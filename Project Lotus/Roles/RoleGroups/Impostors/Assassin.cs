using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Subroles;
using UnityEngine;
using Lotus.Roles.Builtins;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Factions.Impostors;
using Lotus.Roles.Internals.Trackers;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Assassin : GuesserRole, ISabotagerRole
{
    public bool CanSabotage() => true;

    [RoleAction(LotusActionType.Attack)]
    public virtual bool TryKill(PlayerControl target)
    {
        InteractionResult result = MyPlayer.InteractWith(target, LotusInteraction.FatalInteraction.Create(this));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));
        return result is InteractionResult.Proceed;
    }
    protected override bool CanGuessRole(CustomRole role) => role.Faction.GetType() != typeof(ImpostorFaction);

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(Color.red)
            .Faction(FactionInstances.Impostors)
            .RoleAbilityFlags(RoleAbilityFlag.IsAbleToKill)
            .VanillaRole(RoleTypes.Impostor);
}