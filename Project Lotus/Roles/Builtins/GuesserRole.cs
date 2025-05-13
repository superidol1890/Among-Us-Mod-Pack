using System;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Trackers;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Lotus.GameModes.Standard;
using Lotus.API.Vanilla.Meetings;
using Lotus.Roles.Managers.Interfaces;
using System.Linq;
using Lotus.Roles.Subroles;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using System.Collections.Generic;

namespace Lotus.Roles.Builtins;

public class GuesserRole : CustomRole
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(GuesserRole));

    private int guessesPerMeeting;
    private bool followGuesserSettings;

    private bool hasMadeGuess;
    private byte guessingPlayer = byte.MaxValue;
    private bool skippedVote;
    private CustomRole? guessedRole;
    private int guessesThisMeeting;

    protected int CorrectGuesses;
    protected string? GuesserMessage;

    protected MeetingPlayerSelector voteSelector = null!;

    protected override void PostSetup()
    {
        voteSelector = new();
        base.PostSetup();
    }

    [RoleAction(LotusActionType.RoundStart)]
    [RoleAction(LotusActionType.RoundEnd)]
    public void ResetPreppedPlayer()
    {
        hasMadeGuess = false;
        voteSelector.Reset();
        guessingPlayer = byte.MaxValue;
        skippedVote = false;
        guessedRole = null;
        GuesserMessage = null;
        guessesThisMeeting = 0;
    }

    [RoleAction(LotusActionType.Vote)]
    public void SelectPlayerToGuess(Optional<PlayerControl> player, MeetingDelegate _, ActionHandle handle)
    {
        if (skippedVote || hasMadeGuess) return;
        handle.Cancel();
        VoteResult result = voteSelector.CastVote(player);
        switch (result.VoteResultType)
        {
            case VoteResultType.None:
                break;
            case VoteResultType.Skipped:
                skippedVote = true;
                break;
            case VoteResultType.Selected:
                guessingPlayer = result.Selected;
                PlayerControl? targetPlayer = Players.FindPlayerById(result.Selected);
                if (targetPlayer != null)
                {
                    CancelGuessReason reason = CanGuessPlayer(targetPlayer);
                    if (reason is CancelGuessReason.None) GuesserHandler(Guesser.Translations.PickedPlayerText.Formatted(targetPlayer.name)).Send(MyPlayer);
                    else
                    {
                        GuesserHandler(reason switch
                        {
                            CancelGuessReason.RoleSpecificReason => Guesser.Translations.CantGuessBecauseOfRole.Formatted(targetPlayer.name),
                            CancelGuessReason.Teammate => Guesser.Translations.CantGuessTeammate.Formatted(targetPlayer.name),
                            CancelGuessReason.CanSeeRole => Guesser.Translations.CanSeeRole.Formatted(targetPlayer.name),
                            _ => Guesser.Translations.ErrorCompletingGuess
                        }).Send(MyPlayer);
                        guessingPlayer = byte.MaxValue;
                    }
                }
                else guessingPlayer = byte.MaxValue;
                break;
            case VoteResultType.Confirmed:
                if (guessedRole == null)
                {
                    voteSelector.Reset();
                    voteSelector.CastVote(player);
                }
                else hasMadeGuess = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!hasMadeGuess) return;

        if (++guessesThisMeeting < guessesPerMeeting)
        {
            hasMadeGuess = false;
            voteSelector.Reset();
        }

        PlayerControl? guessed = Players.FindPlayerById(guessingPlayer);
        if (guessed == null || guessedRole == null)
        {
            GuesserHandler(Guesser.Translations.ErrorCompletingGuess).Send(MyPlayer);
            ResetPreppedPlayer();
            return;
        }

        if (guessed.PrimaryRole().GetType() == guessedRole.GetType() || guessed.GetSubroles().Any(s => s.GetType() == guessedRole.GetType()))
        {
            GuesserMessage = Guesser.Translations.GuessAnnouncementMessage.Formatted(guessed.name);
            MyPlayer.InteractWith(guessed, LotusInteraction.FatalInteraction.Create(this));
            CorrectGuesses++;
        }
        else HandleBadGuess();
    }

    [RoleAction(LotusActionType.MeetingEnd, ActionFlag.WorksAfterDeath)]
    public void CheckRevive()
    {
        if (GuesserMessage != null) GuesserHandler(GuesserMessage).Send();
    }

    [RoleAction(LotusActionType.Chat)]
    public void DoGuesserVoting(string message, GameState state, bool isAlive)
    {
        if (!isAlive) return;
        if (state is not GameState.InMeeting) return;
        log.Debug($"Message: {message} - Guessing player: {guessingPlayer}");
        if (guessingPlayer == byte.MaxValue) return;
        if (!(message.StartsWith("/role") || message.StartsWith("/r"))) return;
        string[] split = message.Replace("/role", "/r").Split(" ");
        if (split.Length == 1)
        {
            GuesserHandler(Guesser.Translations.TypeRText).Send(MyPlayer);
            return;
        }

        string roleName = split[1..].Fuse(" ").Trim();
        var allRoles = StandardRoles.Instance.AllRoles.Where(r => (r.Count > 0 | r.RoleFlags.HasFlag(RoleFlag.RemoveRoleMaximum)) && r.Chance > 0 || r.RoleFlags.HasFlag(RoleFlag.VariationRole)
            || r.RoleFlags.HasFlag(RoleFlag.TransformationRole));
        Optional<CustomRole> role = allRoles.FirstOrOptional(r => r.RoleName.ToLower().Contains(roleName.ToLower()))
            .CoalesceEmpty(() => allRoles.FirstOrOptional(r => r.Aliases.Contains(roleName)));
        log.Debug($"c4 - exists: {role.Exists()} name: {(role.Exists() ? role.Get().RoleName : roleName)}");
        if (!role.Exists())
        {
            GuesserHandler(Guesser.Translations.UnknownRole.Formatted(roleName)).Send(MyPlayer);
            return;
        }

        guessedRole = role.Get();

        if (!CanGuessRole(guessedRole))
        {
            GuesserHandler(Guesser.Translations.CantGuessRoleBecauseOfRole.Formatted(guessedRole.RoleName)).Send(MyPlayer);
            guessedRole = null;
            return;
        }
        if (!followGuesserSettings)
        {
            GuesserHandler(Guesser.Translations.PickedRoleText.Formatted(Players.FindPlayerById(guessingPlayer)?.name, guessedRole.RoleName)).Send(MyPlayer);
            return;
        }
        int setting = -1;
        Guesser.RoleTypeBuilders.FirstOrOptional(b => b.predicate(guessedRole)).IfPresent(rtb => setting = Guesser.RoleTypeSettings[Guesser.RoleTypeBuilders.IndexOf(rtb)]);
        if (setting == -1 || setting == 2) setting = Guesser.CanGuessDictionary.GetValueOrDefault(guessedRole.GetType(), -1);

        if (setting == 1) GuesserHandler(Guesser.Translations.PickedRoleText.Formatted(Players.FindPlayerById(guessingPlayer)?.name, guessedRole.RoleName)).Send(MyPlayer);
        else
        {
            GuesserHandler(Guesser.Translations.CantGuessRole.Formatted(guessedRole.RoleName)).Send(MyPlayer);
            guessedRole = null;
        }
    }

    protected virtual void HandleBadGuess()
    {
        GuesserMessage = Guesser.Translations.GuessAnnouncementMessage.Formatted(MyPlayer.name);
        MyPlayer.InteractWith(MyPlayer, LotusInteraction.FatalInteraction.Create(this));
    }
    protected virtual CancelGuessReason CanGuessPlayer(PlayerControl targetPlayer)
    {
        bool canSeeRole = false;
        RoleComponent? roleComponent = targetPlayer.NameModel().GetComponentHolder<RoleHolder>().LastOrDefault();
        if (roleComponent != null) canSeeRole = roleComponent.Viewers().Any(p => p == MyPlayer);
        return canSeeRole ? CancelGuessReason.CanSeeRole : CancelGuessReason.None;
    }
    protected virtual bool CanGuessRole(CustomRole role) => true;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Guesses per Meeting", Guesser.Translations.Options.GuesserPerMeeting)
                .AddIntRange(1, 10, 1, 0)
                .BindInt(i => guessesPerMeeting = i)
                .Build())
            .SubOption(sub => sub.KeyName("Follow Guesser Settings", Guesser.Translations.Options.FollowGuesserSettings)
                .AddBoolean()
                .BindBool(b => followGuesserSettings = b)
                .Build());

    protected ChatHandler GuesserHandler(string message) => ChatHandler.Of(message, RoleColor.Colorize(Guesser.Translations.GuesserTitle)).LeftAlign();

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier
        .VanillaRole(RoleTypes.Crewmate);
}