using Lotus.GameModes.Colorwars.Factions;
using UnityEngine;

namespace Lotus.GameModes.CTF.Factions;

public class CTFTeamFaction : ColorFaction
{
    public CTFTeamFaction(int teamId, Color teamColor) : base(teamId, teamColor)
    {

    }

    public override bool CanSeeRole(PlayerControl player) => true;
}