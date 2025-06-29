namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// The event that is invoked when the intro cutscene is shown. Non cancelable.
/// </summary>
public class IntroBeginEvent : MiraEvent
{
    /// <summary>
    /// Gets the instance of the IntroCutscene.
    /// </summary>
    public IntroCutscene IntroCutscene { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroBeginEvent"/> class.
    /// </summary>
    /// <param name="introCutscene">The intro cutscene.</param>
    public IntroBeginEvent(IntroCutscene introCutscene)
    {
        IntroCutscene = introCutscene;
    }
}
