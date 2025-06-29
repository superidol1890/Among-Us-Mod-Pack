namespace MiraAPI.Events.Vanilla.Usables;

/// <summary>
/// Event for if a <see cref="PlayerControl"/> can use an <see cref="IUsable"/> from Vanilla Among Us. Will always be ran locally.
/// </summary>
public class PlayerCanUseEvent : PlayerUseEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerCanUseEvent"/> class.
    /// </summary>
    /// <param name="usable">The IUsable.</param>
    public PlayerCanUseEvent(IUsable usable) : base(usable)
    {
    }
}
