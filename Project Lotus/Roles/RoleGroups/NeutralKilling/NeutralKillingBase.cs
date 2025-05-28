using Lotus.Factions;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Enums;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.NeutralKilling;
// IModdable
public class NeutralKillingBase : Vanilla.Impostor
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .SpecialType(SpecialType.NeutralKilling)
            .Faction(FactionInstances.Neutral)
            .RoleColor(Color.gray);
}