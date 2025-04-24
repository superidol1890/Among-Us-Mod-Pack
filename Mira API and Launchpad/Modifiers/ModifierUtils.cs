using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.Modifiers;

/// <summary>
/// Utilities to make handling modifiers in-game easier.
/// </summary>
public static class ModifierUtils
{
    /// <summary>
    /// Gets a list of all active in-game modifiers.
    /// </summary>
    /// <param name="predicate">Select if modifier is valid to be added to list.</param>
    /// <typeparam name="T">The modifier type.</typeparam>
    /// <returns>A list of modifiers.</returns>
    public static IEnumerable<T> GetActiveModifiers<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return PlayerControl.AllPlayerControls.ToArray().SelectMany(x => x.GetModifiers<T>(predicate)).OfType<T>();
    }

    /// <summary>
    /// Gets all players with a certain modifier.
    /// </summary>
    /// <param name="predicate">Select if modifier is valid.</param>
    /// <typeparam name="T">The modifier type.</typeparam>
    /// <returns>A list of players with that modifier.</returns>
    public static IEnumerable<PlayerControl> GetPlayersWithModifier<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasModifier<T>(predicate));
    }
}
