using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.API;
using Lotus.API.Player;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class ProtectEvent : ITargetedEvent, IRoleEvent
{
    private FrozenPlayer? protector;
    private Optional<CustomRole> protectorRole;

    private FrozenPlayer? target;
    private Optional<CustomRole> targetRole;

    private Timestamp timestamp = new();

    public ProtectEvent(PlayerControl protector, PlayerControl target)
    {
        this.protector = Game.MatchData.GetFrozenPlayer(protector);
        protectorRole = Optional<CustomRole>.Of(protector.PrimaryRole());
        this.target = Game.MatchData.GetFrozenPlayer(target);
        targetRole = Optional<CustomRole>.Of(target.PrimaryRole());
    }

    public FrozenPlayer Player() => protector;

    public Optional<CustomRole> RelatedRole() => protectorRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public string Message() => $"{Game.GetName(protector)} began protecting {Game.GetName(target)}";

    public FrozenPlayer Target() => target;

    public Optional<CustomRole> TargetRole() => targetRole;
}