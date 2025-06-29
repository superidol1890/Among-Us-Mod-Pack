namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Invoked before RpcEndGame is called, allowing cancellation of the game end entirely.
/// </summary>
public class BeforeGameEndEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the reason for the game end.
    /// </summary>
    public GameOverReason Reason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeGameEndEvent"/> class.
    /// </summary>
    /// <param name="reason">The reason for the game end.</param>
    public BeforeGameEndEvent(GameOverReason reason)
    {
        Reason = reason;
    }
}
