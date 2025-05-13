using Lotus.API.Player;
using Lotus.Roles;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public interface ITargetedEvent : IHistoryEvent
{
    public FrozenPlayer Target();

    public Optional<CustomRole> TargetRole();
}