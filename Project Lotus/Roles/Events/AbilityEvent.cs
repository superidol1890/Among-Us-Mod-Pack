using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Managers.History;
using Lotus.Managers.History.Events;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Events;

public abstract class AbilityEvent : IRoleEvent
{
    private FrozenPlayer? user;
    private Optional<CustomRole> role;

    private Timestamp timestamp = new();
    private bool completion;

    public AbilityEvent(PlayerControl user, bool completed = true)
    {
        this.user = Game.MatchData.GetFrozenPlayer(user);
        role = Optional<CustomRole>.Of(user.PrimaryRole());
        completion = completed;
    }

    public FrozenPlayer Player() => user;

    public Optional<CustomRole> RelatedRole() => role;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => completion;

    public abstract string Message();
}