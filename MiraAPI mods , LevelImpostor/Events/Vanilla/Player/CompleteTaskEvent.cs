namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// The event that is invoked when a player completes a task. Non cancelable.
/// </summary>
public class CompleteTaskEvent : MiraEvent
{
    /// <summary>
    /// Gets the instance of the <see cref="PlayerControl"/>.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the instance of the <see cref="PlayerTask"/> that the player completed.
    /// </summary>
    public PlayerTask Task { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteTaskEvent"/> class.
    /// </summary>
    /// <param name="player">The player who completed the task.</param>
    /// <param name="task">The task that the player completed.</param>
    public CompleteTaskEvent(PlayerControl player, PlayerTask task)
    {
        Player = player;
        Task = task;
    }
}
