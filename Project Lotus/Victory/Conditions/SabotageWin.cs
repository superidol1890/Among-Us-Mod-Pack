using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Factions;
using Lotus.Factions.Impostors;
using Lotus.Patches.Systems;
using Lotus.Extensions;
using Lotus.Roles.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.Victory.Conditions;

public class SabotageWin : IWinCondition
{
    [Localized($"{ModConstants.Localization.WinConditions}.{nameof(SabotagedWin)}")]
    public static string SabotagedWin = "Sabotage Win";

    public bool IsConditionMet(out List<PlayerControl> winners)
    {
        winners = null!;
        if (SabotagePatch.CurrentSabotage == null || SabotagePatch.SabotageCountdown > 0 || Math.Abs(SabotagePatch.SabotageCountdown - (-1)) < 0.01) return false;
        ISabotage sabotage = SabotagePatch.CurrentSabotage;
        if (sabotage.SabotageType() is SabotageType.Lights or SabotageType.Communications or SabotageType.Door) return false;

        // List<PlayerControl> eligiblePlayers = Players.GetPlayers().Where(p => p.PrimaryRole().CanSabotage()).ToList();
        List<PlayerControl> eligiblePlayers = Players.GetPlayers().Where(p => p.PrimaryRole() is Roles.RoleGroups.Vanilla.Impostor i && i.CanSabotage()).ToList();
        List<PlayerControl> impostors = eligiblePlayers.Where(p => p.PrimaryRole().Faction is ImpostorFaction).ToList();
        List<PlayerControl> others = eligiblePlayers.Except(impostors).ToList();

        if (impostors.Count >= others.Count)
            winners = impostors;
        else if (sabotage.Caller().Exists())
            winners = eligiblePlayers.Where(p => p.Relationship(sabotage.Caller().Get()) is Relation.SharedWinners or Relation.FullAllies).ToList();
        else if (eligiblePlayers.Count > 0)
            winners = new List<PlayerControl> { eligiblePlayers.GetRandom() };
        else
            winners = new List<PlayerControl>();
        return true;
    }

    public WinReason GetWinReason() => new(ReasonType.Sabotage, SabotagedWin);

    public int Priority() => 3;
}