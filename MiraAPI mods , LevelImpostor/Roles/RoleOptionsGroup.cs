using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Represents a group of roles.
/// </summary>
/// <param name="Name">The group's name.</param>
/// <param name="Color">The group's color.</param>
/// <param name="Priority">The priority to sort by on role setting screen. Negative means first.</param>
public record struct RoleOptionsGroup(string Name, Color Color, int Priority = 0)
{
    /// <summary>
    /// Gets the default group for crewmates.
    /// </summary>
    public static RoleOptionsGroup Crewmate { get; } = new("Crewmate", Palette.CrewmateBlue, -2);

    /// <summary>
    /// Gets the default group for impostors.
    /// </summary>
    public static RoleOptionsGroup Impostor { get; } = new("Impostor", Palette.ImpostorRed, -1);

    /// <summary>
    /// Gets the default group for neutral roles.
    /// </summary>
    public static RoleOptionsGroup Neutral { get; } = new("Neutral Roles", Color.gray);
}
