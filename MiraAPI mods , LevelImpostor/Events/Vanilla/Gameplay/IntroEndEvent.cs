namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// The event that is invoked when the intro cutscene has finished playing.
/// </summary>
public class IntroEndEvent(IntroCutscene introCutscene) : MiraEvent
{
    /// <summary>
    /// Gets the instance of the IntroCutscene.
    /// </summary>
    public IntroCutscene IntroCutscene { get; } = introCutscene;
}
