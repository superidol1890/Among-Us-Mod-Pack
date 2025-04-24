using InnerNet;

namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// Event that is invoked when a player leaves the game. Non cancelable.
/// </summary>
public class PlayerLeaveEvent : MiraEvent
{
    /// <summary>
    /// Gets the player who left.
    /// </summary>
    public ClientData ClientData { get; }

    /// <summary>
    /// Gets the reason why the player left.
    /// </summary>
    public DisconnectReasons Reason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerLeaveEvent"/> class.
    /// </summary>
    /// <param name="data">The data of the player who left.</param>
    /// <param name="reason">The reason why the player left.</param>
    public PlayerLeaveEvent(ClientData data, DisconnectReasons reason)
    {
        ClientData = data;
        Reason = reason;
    }
}
