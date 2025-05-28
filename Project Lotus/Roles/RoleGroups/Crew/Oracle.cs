using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Chat;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Impl;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Lotus.API.Player;
using Lotus.API.Vanilla.Meetings;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Trackers;

namespace Lotus.Roles.RoleGroups.Crew;

public class Oracle : Crewmate, IInfoResender
{
    private static ColorGradient _oracleGradient = new(new Color(0.49f, 0.57f, 0.84f), new Color(0.67f, 0.36f, 0.76f));

    private Optional<byte> selectedPlayer = Optional<byte>.Null();
    private bool targetLockedIn;
    private bool initialSkip;

    [NewOnSetup] private MeetingPlayerSelector voteSelector = null!;

    public void ResendMessages() => CHandler().Message(Translations.VotePlayerInfo).Send(MyPlayer);

    [RoleAction(LotusActionType.RoundEnd)]
    private void OracleSendMessage()
    {
        initialSkip = false;
        if (selectedPlayer.Exists()) return;
        CHandler().Message(Translations.VotePlayerInfo).Send(MyPlayer);
        voteSelector.Reset();
    }

    [RoleAction(LotusActionType.Vote)]
    private void OracleLockInTarget(Optional<PlayerControl> target, MeetingDelegate _, ActionHandle handle)
    {
        if (targetLockedIn || initialSkip) return;
        handle.Cancel();
        VoteResult result = voteSelector.CastVote(target);
        switch (result.VoteResultType)
        {
            case VoteResultType.None:
                break;
            // case VoteResultType.Unselected:
            case VoteResultType.Selected:
                selectedPlayer = target.Map(p => p.PlayerId);
                CHandler().Message($"{Translations.SelectRole.Formatted(target.Get().name)}\n{Translations.SkipMessage}").Send(MyPlayer);
                break;
            case VoteResultType.Skipped:
                if (!targetLockedIn)
                {
                    selectedPlayer = Optional<byte>.Null();
                    CHandler().Message(Translations.VoteNormallyMessage).Send(MyPlayer);
                    initialSkip = true;
                }
                break;
            case VoteResultType.Confirmed:
                targetLockedIn = true;
                CHandler().Message(Translations.VoteNormallyMessage).Send(MyPlayer);
                break;
        }
    }

    [RoleAction(LotusActionType.PlayerDeath)]
    private void OracleDies()
    {
        if (!selectedPlayer.Exists()) return;
        PlayerControl target = Utils.GetPlayerById(selectedPlayer.Get())!;
        if (target == null) return; // If the target no longer exists.
        target.NameModel().GetComponentHolder<RoleHolder>().LastOrDefault(c => c.ViewMode() is ViewMode.Replace)?.SetViewerSupplier(() => Players.GetAllPlayers().ToList());

        string roleName = _oracleGradient.Apply(target.PrimaryRole().RoleName);

        target.NameModel().GetComponentHolder<RoleHolder>().Add(new RoleComponent(new LiveString(_ => roleName), Game.InGameStates, ViewMode.Replace));
        CHandler().Message(Translations.RevealMessage, target.name, roleName).Send();
    }

    [RoleAction(LotusActionType.Disconnect, ActionFlag.GlobalDetector)]
    private void TargetDisconnected(PlayerControl dcPlayer)
    {
        if (!selectedPlayer.Exists() || selectedPlayer.Get() != dcPlayer.PlayerId) return;
        selectedPlayer = Optional<byte>.Null();
        targetLockedIn = false;
    }

    private ChatHandler CHandler() => ChatHandler.Of(title: _oracleGradient.Apply(Translations.OracleMessageTitle)).LeftAlign();

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.78f, 0.55f, 0.82f));

    [Localized(nameof(Oracle))]
    private static class Translations
    {
        [Localized(nameof(OracleMessageTitle))]
        public static string OracleMessageTitle = "Oracle Ability";

        [Localized(nameof(VotePlayerInfo))]
        public static string VotePlayerInfo = "Vote to select a player to reveal on your death. You can vote someone else to change to that person.\nAfter confirming your target cannot be changed.";

        [Localized(nameof(SelectRole))]
        public static string SelectRole = "You have selected: {0}";

        [Localized(nameof(UnselectRole))]
        public static string UnselectRole = "You have unselected: {0}";

        [Localized(nameof(VoteNormallyMessage))]
        public static string VoteNormallyMessage = "You may now vote normally.";

        [Localized(nameof(SkipMessage))]
        public static string SkipMessage = "Vote the same player to continue.";

        [Localized(nameof(RevealMessage))]
        public static string RevealMessage = "The Oracle has revealed to all that {0} is the {1}";
    }
}