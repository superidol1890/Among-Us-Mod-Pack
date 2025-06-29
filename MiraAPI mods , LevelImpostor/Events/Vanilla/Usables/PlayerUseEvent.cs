namespace MiraAPI.Events.Vanilla.Usables;

/// <summary>
/// Event for <see cref="PlayerControl"/> using a <see cref="IUsable"/> from Vanilla Among Us. Will always be ran locally.
/// </summary>
public class PlayerUseEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of <see cref="IUsable"/> that was used.
    /// </summary>
    public IUsable Usable { get; }

    /// <summary>
    /// Gets a value indicating whether the IUsable is a <see cref="Console"/>, <see cref="MapConsole"/>, or <see cref="SystemConsole"/>.
    /// </summary>
    public bool IsPrimaryConsole { get; }

    /// <summary>
    /// Gets a value indicating whether the IUsable is a <see cref="Vent"/>.
    /// </summary>
    public bool IsVent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerUseEvent"/> class.
    /// </summary>
    /// <param name="usable">The IUsable.</param>
    public PlayerUseEvent(IUsable usable)
    {
        Usable = usable;

        IsPrimaryConsole = usable.TryCast<Console>() || usable.TryCast<SystemConsole>() || usable.TryCast<MapConsole>();
        IsVent = usable.TryCast<Vent>();
    }
}
