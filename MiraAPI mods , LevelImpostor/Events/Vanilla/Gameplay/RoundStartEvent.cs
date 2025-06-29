namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Start Round event, invoked on IntroCutscene.OnDestroy and ExileController.WrapUp.
/// </summary>
public class RoundStartEvent : MiraEvent
{
    /// <summary>
    /// Gets a value indicating whether the event was triggered by the IntroCutscene or ExileController.
    /// </summary>
    public bool TriggeredByIntro { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundStartEvent"/> class.
    /// </summary>
    /// <param name="triggeredByIntro">Whether the event was triggered by the intro or not.</param>
    public RoundStartEvent(bool triggeredByIntro)
    {
        TriggeredByIntro = triggeredByIntro;
    }
}
