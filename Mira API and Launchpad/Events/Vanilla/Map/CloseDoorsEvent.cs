namespace MiraAPI.Events.Vanilla.Map;

/// <summary>
/// Event fired when a player closes the doors in a room.
/// </summary>
public class CloseDoorsEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the room that the doors were closed in.
    /// </summary>
    public SystemTypes Room { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloseDoorsEvent"/> class.
    /// </summary>
    /// <param name="room">The room that was closed.</param>
    public CloseDoorsEvent(SystemTypes room)
    {
        Room = room;
    }
}
