namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when a player is ejected. Non-cancelable.
/// </summary>
public class EjectionEvent : MiraEvent
{
    /// <summary>
    /// Gets the instance of the ExileController.
    /// </summary>
    public ExileController ExileController { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EjectionEvent"/> class.
    /// </summary>
    /// <param name="controller">The exile controller.</param>
    public EjectionEvent(ExileController controller)
    {
        ExileController = controller;
    }
}
