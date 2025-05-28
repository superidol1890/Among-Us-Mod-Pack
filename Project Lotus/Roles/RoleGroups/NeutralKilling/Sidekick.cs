using Lotus.API;
using Lotus.Roles.Factions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Sidekick : NeutralKillingBase
{
    private Jackal parentJackal;

    public bool ImpostorVision;
    public bool CanVentOverride;
    public bool CanSabotageOverride;

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    public void SetParentJackal(Jackal jackal)
    {
        this.parentJackal = jackal;
    }

    public override void HandleDisconnect() => parentJackal.OnSidekickDisconnect();
    public override bool CanSabotage() => CanSabotageOverride;

    protected override RoleType GetRoleType() => RoleType.DontShow;
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(Jackal.JackalColor)
            .CanVent(CanVentOverride)
            .Faction(JackalFaction.Instance)
            .RoleFlags(RoleFlag.TransformationRole)
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod(), () => !ImpostorVision);
}