using Lotus.API.Vanilla.Meetings;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

public partial class Alchemist
{
    [RoleAction(LotusActionType.RoundStart)]
    private void ResetCertainEffects()
    {
        ExtraVotes = 0;
        QuickFixSabotage = false;
    }

    [RoleAction(LotusActionType.Interaction)]
    private void ProtectionEffect(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (!IsProtected) return;
        if (interaction.Intent is INeutralIntent or IHelpfulIntent) return;
        IsProtected = false;
        handle.Cancel();
    }

    [RoleAction(LotusActionType.Vote)]
    private void IncreasedVoting(Optional<PlayerControl> votedFor, MeetingDelegate meetingDelegate)
    {
        if (!votedFor.Exists()) return;
        for (int i = 0; i < ExtraVotes; i++) meetingDelegate.CastVote(MyPlayer, votedFor);
    }

    [RoleAction(LotusActionType.SabotagePartialFix)]
    private void SabotageQuickFix(ISabotage sabotage)
    {
        sabotage.Fix(MyPlayer);
        QuickFixSabotage = false;
    }
}