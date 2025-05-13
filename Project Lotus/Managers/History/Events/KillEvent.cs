using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Roles;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class KillEvent : IKillEvent
{
    private FrozenPlayer? killer;
    private Optional<CustomRole> killerRole;

    private FrozenPlayer? victim;
    private Optional<CustomRole> victimRole;

    private bool successful;
    private Timestamp timestamp = new();

    public KillEvent(PlayerControl killer, PlayerControl victim, bool successful = true)
    {
        this.killer = Game.MatchData.GetFrozenPlayer(killer);
        killerRole = Optional<CustomRole>.Of(killer.PrimaryRole());
        this.victim = Game.MatchData.GetFrozenPlayer(victim);
        victimRole = Optional<CustomRole>.Of(victim.PrimaryRole());
        this.successful = successful;
    }

    public FrozenPlayer Player() => this.killer;

    public Optional<CustomRole> RelatedRole() => this.killerRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => successful;

    public virtual string Message() => $"{Game.GetName(killer)} {(successful ? "killed" : "tried to kill")} {Game.GetName(victim)}.";

    public FrozenPlayer Target() => victim;

    public Optional<CustomRole> TargetRole() => victimRole;
}