namespace MiraAPI.Events.Vanilla.Usables;

/// <summary>
/// Event that is invoked when a player exits a vent. This event is cancelable.
/// </summary>
public class ExitVentEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the player that is exiting the vent.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the vent that the player is exiting.
    /// </summary>
    public Vent? Vent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExitVentEvent"/> class.
    /// </summary>
    /// <param name="player">The player who is exiting the vent.</param>
    /// <param name="vent">The vent being exited from.</param>
    public ExitVentEvent(PlayerControl player, Vent? vent)
    {
        Player = player;
        Vent = vent;
    }
}
