namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Event that is invoked after a player is murdered. Only called after a successful murder. This event is not cancelable.
/// </summary>
public class AfterMurderEvent : MiraEvent
{
    /// <summary>
    /// Gets the player that killed the target.
    /// </summary>
    public PlayerControl Source { get; }

    /// <summary>
    /// Gets the player that was killed.
    /// </summary>
    public PlayerControl Target { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AfterMurderEvent"/> class.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The killed player.</param>
    public AfterMurderEvent(PlayerControl source, PlayerControl target)
    {
        Source = source;
        Target = target;
    }
}
