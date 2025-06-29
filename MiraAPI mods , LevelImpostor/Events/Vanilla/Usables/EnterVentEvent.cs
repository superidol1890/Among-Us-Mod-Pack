namespace MiraAPI.Events.Vanilla.Usables;

/// <summary>
/// Event that is invoked when a player enters a vent. This event is cancelable.
/// </summary>
public class EnterVentEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the player that is entering the vent.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the vent that the player is entering.
    /// </summary>
    public Vent? Vent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnterVentEvent"/> class.
    /// </summary>
    /// <param name="player">The player that is entering the vent.</param>
    /// <param name="vent">The vent being entered.</param>
    public EnterVentEvent(PlayerControl player, Vent? vent)
    {
        Player = player;
        Vent = vent;
    }
}
