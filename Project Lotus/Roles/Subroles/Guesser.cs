using System;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Extensions;
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
using UnityEngine;
using System.Collections.Generic;
using Lotus.Utilities;
using Lotus.Options;
using VentLib.Options;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using MonoMod.Utils;
using Lotus.Factions.Impostors;
using Lotus.Factions.Crew;
using Lotus.Factions.Neutrals;
using Lotus.Factions.Undead;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Components;

namespace Lotus.Roles.Subroles;

public class Guesser : Subrole
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Guesser));
    [NewOnSetup(true)] private MeetingPlayerSelector voteSelector  = new();

    public override string Identifier() => "⌘";

    // ctrl+c, ctrl+ --- BOOM! we have options :)
    public static Dictionary<Type, int> CanGuessDictionary = new();
    public static Dictionary<Type, int> FactionMaxDictionary = new();

    public static List<(Func<CustomRole, bool> predicate, GameOptionBuilder builder)> RoleTypeBuilders = new()
    {
        (r => r.Faction.GetType() == typeof(ImpostorFaction), new GameOptionBuilder()
            .KeyName("Impostor Settings", TranslationUtil.Colorize(Translations.Options.ImpostorSetting, Color.red))
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(0).Color(Color.red).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(1).Color(Color.green).Build())
            .Value(v => v.Text(GeneralOptionTranslations.CustomText).Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build())
            .ShowSubOptionPredicate(i => (int)i == 2)),
        (r => r.Faction is Madmates, new GameOptionBuilder()
            .KeyName("Madmates Settings", TranslationUtil.Colorize(Translations.Options.MadmateSetting, ModConstants.Palette.MadmateColor))
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(0).Color(Color.red).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(1).Color(Color.green).Build())
            .Value(v => v.Text(GeneralOptionTranslations.CustomText).Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build())
            .ShowSubOptionPredicate(i => (int)i == 2)),
        (r => r.Faction is Crewmates, new GameOptionBuilder()
            .KeyName("Crewmate Settings", TranslationUtil.Colorize(Translations.Options.CrewmateSetting, ModConstants.Palette.CrewmateColor))
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(0).Color(Color.red).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(1).Color(Color.green).Build())
            .Value(v => v.Text(GeneralOptionTranslations.CustomText).Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build())
            .ShowSubOptionPredicate(i => (int)i == 2)),
        (r => r.SpecialType is SpecialType.NeutralKilling or SpecialType.Undead, new GameOptionBuilder()
            .KeyName("Neutral Killing Settings", TranslationUtil.Colorize(Translations.Options.NeutralKillingSetting, ModConstants.Palette.NeutralColor, ModConstants.Palette.KillingColor))
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(0).Color(Color.red).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(1).Color(Color.green).Build())
            .Value(v => v.Text(GeneralOptionTranslations.CustomText).Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build())
            .ShowSubOptionPredicate(i => (int)i == 2)),
        (r => r.SpecialType is SpecialType.Neutral, new GameOptionBuilder()
            .KeyName("Neutral Passive Settings", TranslationUtil.Colorize(Translations.Options.NeutralPassiveSetting, ModConstants.Palette.NeutralColor, ModConstants.Palette.PassiveColor))
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(0).Color(Color.red).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(1).Color(Color.green).Build())
            .Value(v => v.Text(GeneralOptionTranslations.CustomText).Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build())
            .ShowSubOptionPredicate(i => (int)i == 2)),
        (r => r.RoleFlags.HasFlag(RoleFlag.IsSubrole), new GameOptionBuilder()
            .KeyName("Modifier Settings", TranslationUtil.Colorize(Translations.Options.SubroleSetting, ModConstants.Palette.ModifierColor))
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(0).Color(Color.red).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(1).Color(Color.green).Build())
            .Value(v => v.Text(GeneralOptionTranslations.CustomText).Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build())
            .ShowSubOptionPredicate(i => (int)i == 2))
    };
    public static List<int> RoleTypeSettings = new() { 0, 0, 0, 0, 0, 0 };

    private bool restrictToNonVotingRoles;
    private bool canGuessTeammates;
    private int guessesPerMeeting;

    private bool hasMadeGuess;
    private byte guessingPlayer = byte.MaxValue;
    private bool skippedVote;
    private CustomRole? guessedRole;
    private int guessesThisMeeting;

    protected int CorrectGuesses;
    protected string? GuesserMessage;

    public Guesser()
    {
        StandardRoles.Callbacks.Add(PopulateGuesserSettings);
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
                    if (reason is CancelGuessReason.None) GuesserHandler(Translations.PickedPlayerText.Formatted(targetPlayer.name)).Send(MyPlayer);
                    else
                    {
                        GuesserHandler(reason switch
                        {
                            CancelGuessReason.RoleSpecificReason => Translations.CantGuessBecauseOfRole.Formatted(targetPlayer.name),
                            CancelGuessReason.Teammate => Translations.CantGuessTeammate.Formatted(targetPlayer.name),
                            CancelGuessReason.CanSeeRole => Translations.CanSeeRole.Formatted(targetPlayer.name),
                            _ => Translations.ErrorCompletingGuess
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
            GuesserHandler(Translations.ErrorCompletingGuess).Send(MyPlayer);
            ResetPreppedPlayer();
            return;
        }

        if (guessed.PrimaryRole().GetType() == guessedRole.GetType() || guessed.GetSubroles().Any(s => s.GetType() == guessedRole.GetType()))
        {
            GuesserMessage = Translations.GuessAnnouncementMessage.Formatted(guessed.name);
            MyPlayer.InteractWith(guessed, LotusInteraction.FatalInteraction.Create(this));
            CorrectGuesses++;
        }
        else HandleBadGuess();
    }

    [RoleAction(LotusActionType.MeetingEnd, ActionFlag.WorksAfterDeath)]
    public void CheckRevive()
    {
        if (GuesserMessage != null)
        {
            GuesserHandler(GuesserMessage).Send();
            GuesserMessage = null;
        }
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
            GuesserHandler(Translations.TypeRText).Send(MyPlayer);
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
            GuesserHandler(Translations.UnknownRole.Formatted(roleName)).Send(MyPlayer);
            return;
        }

        guessedRole = role.Get();

        if (!CanGuessRole(guessedRole))
        {
            GuesserHandler(Translations.CantGuessRoleBecauseOfRole.Formatted(guessedRole.RoleName)).Send(MyPlayer);
            guessedRole = null;
            return;
        }

        int setting = -1;
        RoleTypeBuilders.FirstOrOptional(b => b.predicate(guessedRole)).IfPresent(rtb => setting = RoleTypeSettings[RoleTypeBuilders.IndexOf(rtb)]);
        if (setting == -1 || setting == 2) setting = CanGuessDictionary.GetValueOrDefault(guessedRole.GetType(), -1);

        if (setting == 1) GuesserHandler(Translations.PickedRoleText.Formatted(Players.FindPlayerById(guessingPlayer)?.name, guessedRole.RoleName)).Send(MyPlayer);
        else
        {
            GuesserHandler(Translations.CantGuessRole.Formatted(guessedRole.RoleName)).Send(MyPlayer);
            guessedRole = null;
        }
    }
    protected virtual void HandleBadGuess()
    {
        GuesserMessage = Translations.GuessAnnouncementMessage.Formatted(MyPlayer.name);
        MyPlayer.InteractWith(MyPlayer, LotusInteraction.FatalInteraction.Create(this));
    }
    protected virtual CancelGuessReason CanGuessPlayer(PlayerControl targetPlayer)
    {
        if (targetPlayer.PrimaryRole().Faction.GetType() == MyPlayer.PrimaryRole().Faction.GetType())
        {
            if (MyPlayer.PrimaryRole().Faction.CanSeeRole(MyPlayer)) return canGuessTeammates ? CancelGuessReason.None : CancelGuessReason.Teammate;
            else return CancelGuessReason.None;
        }
        bool canSeeRole = false;
        RoleComponent? roleComponent = targetPlayer.NameModel().GetComponentHolder<RoleHolder>().LastOrDefault();
        if (roleComponent != null) canSeeRole = roleComponent.Viewers().Any(p => p == MyPlayer);
        return canSeeRole ? CancelGuessReason.CanSeeRole : CancelGuessReason.None;
    }
    protected virtual bool CanGuessRole(CustomRole role) => true;

    public override CompatabilityMode RoleCompatabilityMode => CompatabilityMode.Blacklisted;
    public override HashSet<Type>? RestrictedRoles()
    {
        HashSet<Type>? restrictedRoles = base.RestrictedRoles();
        if (!restrictToNonVotingRoles) return restrictedRoles;
        // last resort is also a vote related subrole, so just use what they have
        LastResort.IncompatibleRoles.ForEach(r => restrictedRoles?.Add(r));
        return restrictedRoles;
    }
    // If the faction is neutral, use neutral type attribute.
    private Type GetFactionType(IFaction playerFaction) => playerFaction is INeutralFaction
        ? FactionInstances.Neutral.GetType()
        : playerFaction.GetType();

    private int GetAmountOfPeopleOnFaction(Type faction) => Players.GetAlivePlayers().Count(p =>
        GetFactionType(p.PrimaryRole().Faction) == faction && p.GetSubroles().Any(s => s is Guesser));

    public override bool IsAssignableTo(PlayerControl player)
    {
        Type myFaction = GetFactionType(player.PrimaryRole().Faction);

        // Check if their faction already has the max amount of allowed players.
        // If they are maxed out, we don't even call base and just immediately exit.

        if (GetAmountOfPeopleOnFaction(myFaction) >= FactionMaxDictionary.GetValueOrDefault(myFaction, 0))
            return false;

        // Return base as that's the only check.
        // Base checks restricted roles.
        return base.IsAssignableTo(player);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Restricted to Non Vote-Related Roles", Translations.Options.RestrictedToNonVote)
                .AddBoolean()
                .BindBool(b => restrictToNonVotingRoles = b)
                .Build())
            .SubOption(sub => sub
                .KeyName("Guesses per Meeting", Translations.Options.GuesserPerMeeting)
                .AddIntRange(1, ModConstants.MaxPlayers, 1, 0)
                .BindInt(i => guessesPerMeeting = i)
                .Build())
            .SubOption(sub => sub
                .KeyName("Can Guess Teammates", Translations.Options.CanGuessTeammates)
                .AddBoolean(false)
                .BindBool(b => canGuessTeammates = b)
                .Build());

    private void PopulateGuesserSettings()
    {
        Dictionary<Type, IFaction> allFactions = new() {
            {FactionInstances.Impostors.GetType(), FactionInstances.Impostors},
            {FactionInstances.Crewmates.GetType(), FactionInstances.Crewmates},
            {FactionInstances.Neutral.GetType(), FactionInstances.Neutral},
            {FactionInstances.TheUndead.GetType(), FactionInstances.TheUndead}
        };
        allFactions.AddRange(FactionInstances.AddonFactions);
        allFactions.ForEach(kvp =>
        {
            string keyName = Translations.Options.FactionMaxGuessers.Formatted(kvp.Value.Name());
            Option option = new GameOptionBuilder()
                .KeyName(TranslationUtil.Remove(keyName), TranslationUtil.Colorize(keyName, kvp.Value.Color))
                .AddIntRange(0, ModConstants.MaxPlayers, 1, 1)
                .BindInt(i => FactionMaxDictionary[kvp.Key] = i)
                .Build();
            RoleOptions.AddChild(option);
            GlobalRoleManager.RoleOptionManager.Register(option, OptionLoadMode.LoadOrCreate);
        });
        StandardRoles.Instance.AllRoles.OrderBy(r => r.EnglishRoleName).ForEach(r =>
        {
            RoleTypeBuilders.FirstOrOptional(b => b.predicate(r)).Map(i => i.builder)
                .IfPresent(builder =>
                {
                    builder.SubOption(sub => sub.KeyName(r.EnglishRoleName, r.RoleColor.Colorize(r.RoleName))
                        .AddBoolean()
                        .BindBool(b =>
                        {
                            if (b) CanGuessDictionary[r.GetType()] = 1;
                            else CanGuessDictionary[r.GetType()] = 2;
                        })
                        .Build());
                });
        });
        RoleTypeBuilders.ForEach((rtb, index) =>
        {
            rtb.builder.BindInt(i => RoleTypeSettings[index] = i);
            Option option = rtb.builder.Build();
            RoleOptions.AddChild(option);
            GlobalRoleManager.RoleOptionManager.Register(option, OptionLoadMode.LoadOrCreate);
        });
    }
    protected ChatHandler GuesserHandler(string message) => ChatHandler.Of(message, RoleColor.Colorize(Translations.GuesserTitle)).LeftAlign();

    [Localized(nameof(Guesser))]
    public static class Translations
    {
        [Localized(nameof(PickedRoleText))] public static string PickedRoleText = "You are about to guess {0} as {1}. If you are certain about this, vote {0} again to finalize your guess. Otherwise you can pick another player by voting a different player.. OR pick a different role by typing /r [rolename]";
        [Localized(nameof(CantGuessRole))] public static string CantGuessRole = "The role, {0}, was disabled by Host to be guessed. Please use /r [rolename] on a different role. Use /o to check which roles you can guess.";
        [Localized(nameof(CantGuessRoleBecauseOfRole))] public static string CantGuessRoleBecauseOfRole = "Your role prevented you from guessing a player as {0}. You might be able to find out why using /m.";
        [Localized(nameof(FinishedGuessingText))] public static string FinishedGuessingText = "You have confirmed your guess. If you are not dead, you may now vote normally.";
        [Localized(nameof(CantGuessBecauseOfRole))] public static string CantGuessBecauseOfRole = "Your role prevented you from guessing {0}! Try a different player.";
        [Localized(nameof(PickedPlayerText))] public static string PickedPlayerText = "You are guessing {0}'s role. To guess their role type /r [rolename].";
        [Localized(nameof(ErrorCompletingGuess))] public static string ErrorCompletingGuess = "Error completing guess. You may try and guess again.";
        [Localized(nameof(GuessAnnouncementMessage))] public static string GuessAnnouncementMessage = "The guesser has made a guess. {0} died.";
        [Localized(nameof(CantGuessTeammate))] public static string CantGuessTeammate = "{0} is your teammate! You can't guess that player.";
        [Localized(nameof(UnknownRole))] public static string UnknownRole = "Unknown role {0}. You can use /perc to view all enabled roles.";
        [Localized(nameof(CanSeeRole))] public static string CanSeeRole = "You can see {0}'s role! You can't guess that player.";
        [Localized(nameof(TypeRText))] public static string TypeRText = "Please type /r [roleName] to guess that role.";
        [Localized(nameof(Guesser))] public static string GuesserTitle = "Guesser";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(RestrictedToNonVote))] public static string RestrictedToNonVote = "Restricted to Non Vote-Related Roles";
            [Localized(nameof(GuesserPerMeeting))] public static string GuesserPerMeeting = "Guesses per Meeting";
            [Localized(nameof(CanGuessTeammates))] public static string CanGuessTeammates = "Can Guess Teammates";
            [Localized(nameof(FollowGuesserSettings))] public static string FollowGuesserSettings = "Follow Guesser Settings";

            [Localized(nameof(FactionMaxGuessers))] public static string FactionMaxGuessers = "{0}::0 Faction Max Guessers";

            [Localized(nameof(NeutralKillingSetting))] public static string NeutralKillingSetting = "Can Guess Neutral::0 Killing::1";
            [Localized(nameof(NeutralPassiveSetting))] public static string NeutralPassiveSetting = "Can Guess Neutral::0 Passive::1";
            [Localized(nameof(ImpostorSetting))] public static string ImpostorSetting = "Can Guess Impostors::0";
            [Localized(nameof(CrewmateSetting))] public static string CrewmateSetting = "Can Guess Crewmates::0";
            [Localized(nameof(MadmateSetting))] public static string MadmateSetting = "Can Guess Madmates::0";
            [Localized(nameof(SubroleSetting))] public static string SubroleSetting = "Can Guess Subroles::0";
        }
    }


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleFlags(RoleFlag.RemoveRoleMaximum)
            .RoleColor(new Color(.84f, .74f, 0f));
}

public enum CancelGuessReason
{
    None,
    Teammate,
    CanSeeRole,
    RoleSpecificReason
}