using System;

namespace MiraAPI.Networking.Modifiers;

/// <summary>
/// Modifier data for networking.
/// </summary>
/// <param name="TypeId">The ID of the modifier.</param>
/// <param name="UniqueId">The unique ID of the modifier.</param>
/// <param name="Args">Parameters for constructor.</param>
public record struct ModifierData(uint TypeId, Guid UniqueId, object[] Args);
