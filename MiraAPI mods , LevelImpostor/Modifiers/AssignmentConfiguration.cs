namespace MiraAPI.Modifiers;

/// <summary>
/// Represents the assignment configuration for a game modifier.
/// </summary>
/// <param name="DefaultAmount">The default amount of players that can have this modifier in a game.</param>
/// <param name="DefaultChance">The default chance of the modifier being assigned to a player.</param>
/// <param name="CreateAmountOption">Whether Mira should create an amount option for the modifier.</param>
/// <param name="CreateChanceOption">Whether Mira should create a chance option for the modifier.</param>
public record struct AssignmentConfiguration(int DefaultAmount, int DefaultChance, bool CreateAmountOption, bool CreateChanceOption);
