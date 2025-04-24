namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// Event that is invoked when a player dies. Non cancelable.
/// </summary>
public class PlayerDeathEvent : MiraEvent
{
    /// <summary>
    /// Gets the player who died.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the reason the player died.
    /// </summary>
    public DeathReason DeathReason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerDeathEvent"/> class.
    /// </summary>
    /// <param name="player">The player who died.</param>
    /// <param name="reason">The reason the player died.</param>
    public PlayerDeathEvent(PlayerControl player, DeathReason reason)
    {
        Player = player;
        DeathReason = reason;
    }
}
