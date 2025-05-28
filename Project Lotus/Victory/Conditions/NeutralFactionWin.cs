using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Neutrals;
using Lotus.Roles;

namespace Lotus.Victory.Conditions;

public class NeutralFactionWin: IFactionWinCondition
{
    private bool noOneWon = false;
    private List<IFaction> winningFaction = [];

    public List<IFaction> Factions() => winningFaction;

    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = null!;
        if (Game.State is not (GameState.Roaming or GameState.InMeeting)) return false;

        bool didWin = true;
        winningFaction = [];
        List<CustomRole> allAliveRoles = Players.GetAliveRoles().ToList();
        if (!allAliveRoles.Any()) return false; // Fix null ref bug.
        foreach (CustomRole role in allAliveRoles)
        {
            if (role.Faction is not INeutralFaction)
            {
                didWin = false;
                break;
            }

            if (winningFaction.Any())
            {
                if (winningFaction.All(f => f.Relationship(role) is not Relation.None)) winningFaction.Add(role.Faction);
                else
                {
                    didWin = false;
                    break;
                }
            } else winningFaction.Add(role.Faction);
        }

        if (didWin) winningFaction = winningFaction.Distinct().ToList();
        return didWin;
    }

    public WinReason GetWinReason() => noOneWon ? new(ReasonType.NoWinCondition, "No one is alive.") : new(ReasonType.FactionLastStanding);
}