using AmongUs.GameOptions;

namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Event that is invoked after a player's role is set. This event is not cancelable.
/// </summary>
public class SetRoleEvent : MiraEvent
{
    /// <summary>
    /// Gets the player whos role was changed.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the role that the player was set to.
    /// </summary>
    public RoleTypes Role { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetRoleEvent"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="role">The new role.</param>
    public SetRoleEvent(PlayerControl player, RoleTypes role)
    {
        Player = player;
        Role = role;
    }
}
