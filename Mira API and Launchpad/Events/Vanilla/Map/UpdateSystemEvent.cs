namespace MiraAPI.Events.Vanilla.Map;

/// <summary>
/// Cancelable event that is invoked before a system is updated. Usually used for sabotage events.
/// </summary>
public class UpdateSystemEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the type of system that will be updated.
    /// </summary>
    public SystemTypes SystemType { get; }

    /// <summary>
    /// Gets the player that is updating the system.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the byte amount the system is being updated by.
    /// </summary>
    public byte Amount { get; }

    /// <summary>
    ///  Initializes a new instance of the <see cref="UpdateSystemEvent"/> class.
    /// </summary>
    /// <param name="systemType">The SystemType being updated.</param>
    /// <param name="amount">Amount to update System to.</param>
    public UpdateSystemEvent(SystemTypes systemType, PlayerControl player, byte amount)
    {
        SystemType = systemType;
        Player = player;
        Amount = amount;
    }
}
