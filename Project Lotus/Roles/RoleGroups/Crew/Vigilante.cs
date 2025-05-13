using Lotus.Factions;
using Lotus.Factions.Crew;
using Lotus.Logging;
using Lotus.Roles.Builtins;
using Lotus.Roles.RoleGroups.Vanilla;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.Crew;


public class Vigilante : GuesserRole
{
    protected override bool CanGuessRole(CustomRole role) => role.Faction.GetType() != typeof(Crewmates);

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .Faction(FactionInstances.Crewmates)
            .RoleColor(new Color(0.89f, 0.88f, 0.52f));
}