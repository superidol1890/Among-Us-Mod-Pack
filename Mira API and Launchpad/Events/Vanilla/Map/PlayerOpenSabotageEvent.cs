namespace MiraAPI.Events.Vanilla.Map;

/// <summary>
/// Sabotage from Vanilla Among Us.
/// </summary>
public class PlayerOpenSabotageEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the MapBehaviour.
    /// </summary>
    public MapBehaviour MapBehaviour { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerOpenSabotageEvent"/> class.
    /// </summary>
    /// <param name="mapBehaviour">The MapBehaviour.</param>
    public PlayerOpenSabotageEvent(MapBehaviour mapBehaviour)
    {
        MapBehaviour = mapBehaviour;
    }
}
