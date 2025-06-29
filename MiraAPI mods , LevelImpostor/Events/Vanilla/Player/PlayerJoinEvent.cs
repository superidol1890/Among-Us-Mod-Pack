using InnerNet;

namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// Event that is invoked when a player joins the lobby. Non cancelable.
/// </summary>
public class PlayerJoinEvent : MiraEvent
{
    /// <summary>
    /// Gets the player who joined.
    /// </summary>
    public ClientData ClientData { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerJoinEvent"/> class.
    /// </summary>
    /// <param name="data">The data of the player who joined.</param>
    public PlayerJoinEvent(ClientData data)
    {
        ClientData = data;
    }
}
