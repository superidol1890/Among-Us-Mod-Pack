using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Overrides;
using Lotus.RPC;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;
using static Lotus.ModConstants.Palette;
using static Lotus.Roles.RoleGroups.Impostors.IdentityThief.Translations.Options;
using VentLib.Utilities.Optionals;
using Lotus.API.Vanilla.Meetings;

namespace Lotus.Roles.RoleGroups.Impostors;

public class IdentityThief : Impostor
{
    private ShiftingType shiftingType;

    private bool shapeshiftedThisRound;

    private int currentRound;

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (Relationship(target) is Relation.FullAllies) return false;

        LotusInteraction lotusInteraction = new(new FakeFatalIntent(), this);
        InteractionResult result = MyPlayer.InteractWith(target, lotusInteraction);
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));

        if (result is InteractionResult.Halt) return false;

        if (shiftingType is not ShiftingType.UntilMeeting || !shapeshiftedThisRound) MyPlayer.CRpcShapeshift(target, true);

        int curRoundCopy = currentRound;
        if (shiftingType is ShiftingType.KillCooldown) Async.Schedule(() =>
        {
            if (Game.State is GameState.InMeeting || currentRound != curRoundCopy) return;
            MyPlayer.CRpcRevertShapeshift(true);
            shapeshiftedThisRound = false;
        }, KillCooldown);

        ProtectedRpc.CheckMurder(MyPlayer, target);
        shapeshiftedThisRound = true;
        return true;
    }

    [RoleAction(LotusActionType.RoundStart)]
    private void ClearShapeshiftStatus()
    {
        shapeshiftedThisRound = false;
        currentRound++;
    }

    // might conflict with camouflager
    // [RoleAction(LotusActionType.ReportBody, ActionFlag.GlobalDetector, priority: Priority.Low)]
    // private void HandleMeetingCall(PlayerControl reporter, Optional<NetworkedPlayerInfo> reported, ActionHandle handle)
    // {
    //     if (!shapeshiftedThisRound) return;
    //     shapeshiftedThisRound = false;
    //     MyPlayer.CRpcRevertShapeshift(false);
    //     handle.Cancel();
    //     Async.Schedule(() => MeetingPrep.PrepMeeting(reporter, reported.OrElse(null!)), 0.5f);
    // }
    [RoleAction(LotusActionType.RoundEnd, ActionFlag.WorksAfterDeath)]
    private void HandleMeetingCall()
    {
        if (!shapeshiftedThisRound) return;
        shapeshiftedThisRound = false;
        MyPlayer.CRpcRevertShapeshift(false);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Disguise Settings", DisguiseSettings)
                .Value(v => v.Text(UntilKillCooldown).Value(0).Color(GeneralColor3).Build())
                .Value(v => v.Text(UntilNextKill).Value(1).Color(GeneralColor4).Build())
                .Value(v => v.Text(UntilMeeting).Value(2).Color(GeneralColor5).Build())
                .BindInt(b => shiftingType = (ShiftingType)b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.ShapeshiftDuration, 3000f)
            .OptionOverride(Override.ShapeshiftCooldown, 0.001f);

    [Localized(nameof(IdentityThief))]
    internal static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(DisguiseSettings))]
            public static string DisguiseSettings = "Disguise Settings";

            [Localized(nameof(UntilKillCooldown))]
            public static string UntilKillCooldown = "Kill CD";

            [Localized(nameof(UntilNextKill))]
            public static string UntilNextKill = "Next Kill";

            [Localized(nameof(UntilMeeting))]
            public static string UntilMeeting = "Until Meeting";
        }
    }

    private enum ShiftingType
    {
        KillCooldown,
        NextKill,
        UntilMeeting
    }
}