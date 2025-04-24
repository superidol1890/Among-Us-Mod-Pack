namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Event that is invoked before a player is murdered. This event is cancelable.
/// </summary>
public sealed class BeforeMurderEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the player that is killing the target.
    /// </summary>
    public PlayerControl Source { get; }

    /// <summary>
    /// Gets the player that is being killed.
    /// </summary>
    public PlayerControl Target { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeMurderEvent"/> class.
    /// </summary>
    /// <param name="source">The player that is killing the target.</param>
    /// <param name="target">The player that is being killed.</param>
    public BeforeMurderEvent(PlayerControl source, PlayerControl target)
    {
        Source = source;
        Target = target;
    }
}
