using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Managers.History;
using Lotus.Managers.History.Events;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Events;

public abstract class TargetedAbilityEvent : ITargetedEvent, IRoleEvent
{
    private FrozenPlayer? source;
    private Optional<CustomRole> sourceRole;

    private FrozenPlayer? target;
    private Optional<CustomRole> targetRole;

    private Timestamp timestamp = new();
    private bool success;


    public TargetedAbilityEvent(PlayerControl source, PlayerControl target, bool successful = true)
    {
        this.source = Game.MatchData.GetFrozenPlayer(source);
        sourceRole = Optional<CustomRole>.Of(source.PrimaryRole());

        this.target = Game.MatchData.GetFrozenPlayer(target);
        targetRole = Optional<CustomRole>.Of(target.PrimaryRole());
        success = successful;
    }

    public FrozenPlayer Player() => source!;

    public Optional<CustomRole> RelatedRole() => sourceRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => success;

    public abstract string Message();

    public FrozenPlayer Target() => target!;

    public Optional<CustomRole> TargetRole() => targetRole;
}