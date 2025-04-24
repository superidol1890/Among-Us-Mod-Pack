namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// The event that is invoked when the end game screen is shown. Non cancelable.
/// </summary>
public class GameEndEvent : MiraEvent
{
    /// <summary>
    /// Gets the instance of the EndGameManager.
    /// </summary>
    public EndGameManager EndGameManager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEndEvent"/> class.
    /// </summary>
    /// <param name="manager">The instance of the EndGameManager.</param>
    public GameEndEvent(EndGameManager manager)
    {
        EndGameManager = manager;
    }
}
