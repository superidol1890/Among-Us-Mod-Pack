using System.Collections.Generic;
using Lotus.API.Odyssey;

namespace Lotus.Victory.Conditions;

/// <summary>
/// Creates a ManualWin which upon calling Activate() causes a game win
/// By default ManualWins have a priority of 1 meaning they take precedent over standard win conditions
/// </summary>
public class ManualWin: IWinCondition
{
    private List<PlayerControl> winners;
    private WinReason winReason;
    private int priority;

    public static void Activate(PlayerControl player, ReasonType reason, int priority = 0) => new ManualWin(player, reason, priority).Activate();

    public ManualWin(PlayerControl player, ReasonType reason, int priority = 0) : this(new List<PlayerControl> { player }, reason, priority) {}

    public ManualWin(PlayerControl player, WinReason reason, int priority = 0) : this(new List<PlayerControl> { player }, reason, priority) {}

    public ManualWin(List<PlayerControl> players, ReasonType reason, int priority = 0)
    {
        this.winners = players;
        this.winReason = new WinReason(reason);
        this.priority = priority;
    }

    public ManualWin(List<PlayerControl> players, WinReason reason, int priority = 0)
    {
        this.winners = players;
        this.winReason = reason;
        this.priority = priority;
    }

    public void Activate()
    {
        Game.GetWinDelegate().AddWinCondition(this);
        Game.GetWinDelegate().ForceGameWin(this.winners, winReason, this);
    }

    public bool IsConditionMet(out List<PlayerControl> winners)
    {
        winners = this.winners;
        return true;
    }

    public WinReason GetWinReason() => winReason;

    public int Priority() => priority;
}