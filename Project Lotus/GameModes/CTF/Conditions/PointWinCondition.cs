using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Options;
using Lotus.Victory.Conditions;

namespace Lotus.GameModes.CTF.Conditions;

public class PointWinCondition : IWinCondition
{
    private WinReason outputReason;
    public bool IsConditionMet(out List<PlayerControl> winners)
    {
        winners = null!;
        if ((DateTime.Now - Game.MatchData.StartTime).TotalSeconds < ExtraGamemodeOptions.CaptureOptions.GameLength) return false;

        if (CTFGamemode.Team0Score == CTFGamemode.Team1Score)
        {
            winners = new List<PlayerControl>();
            outputReason = new(ReasonType.NoWinCondition, "The game was a tie.");
        }
        else if (CTFGamemode.Team1Score > CTFGamemode.Team0Score)
        {
            winners = Players.GetAllPlayers().Where(p => p.cosmetics.bodyMatProperties.ColorId == 1).ToList();
            outputReason = new(ReasonType.GameModeSpecificWin, "Blue Team had more points than Red Team.");
        }
        else
        {
            winners = Players.GetAllPlayers().Where(p => p.cosmetics.bodyMatProperties.ColorId == 0).ToList();
            outputReason = new(ReasonType.GameModeSpecificWin, "Red Team had more points than Blue Team");
        }

        return true;
    }

    public WinReason GetWinReason() => outputReason;
}