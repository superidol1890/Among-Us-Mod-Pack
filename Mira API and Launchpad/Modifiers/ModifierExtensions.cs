using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Networking;
using MiraAPI.Networking.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace MiraAPI.Modifiers;

/// <summary>
/// Extensions for modifiers.
/// </summary>
public static class ModifierExtensions
{
    /// <summary>
    /// Gets the dictionary to cache ModifierComponents.
    /// </summary>
    public static Dictionary<PlayerControl, ModifierComponent> ModifierComponents { get; } = [];

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="target">The player to add the modifier to.</param>
    /// <param name="typeId">The modifier type ID.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    public static void RpcAddModifier(this PlayerControl target, uint typeId, params object[] args)
    {
        _ = ModifierManager.GetModifierType(typeId) ?? throw new InvalidOperationException(
            $"Modifier with ID {typeId} is not registered.");

        Rpc<AddModifierRpc>.Instance.Send(target, new ModifierData(typeId, Guid.NewGuid(), args));
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="player">The player to add the modifier to.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    public static void RpcAddModifier(this PlayerControl player, Type type, params object[] args)
    {
        var id = ModifierManager.GetModifierTypeId(type) ?? throw new InvalidOperationException(
            $"Modifier {type.Name} is not registered.");

        player.RpcAddModifier(id, args);
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="player">The player to add the modifier to.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <typeparam name="T">The modifier Type.</typeparam>
    public static void RpcAddModifier<T>(this PlayerControl player, params object[] args) where T : BaseModifier
    {
        player.RpcAddModifier(typeof(T), args);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player.
    /// </summary>
    /// <param name="target">The player to remove the modifier from.</param>
    /// <param name="uniqueId">The unique ID of the modifier.</param>
    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(this PlayerControl target, Guid uniqueId)
    {
        target.RemoveModifier(uniqueId);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player by type ID.
    /// </summary>
    /// <param name="target">The player to remove the modifier from.</param>
    /// <param name="typeId">The type ID of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    public static void RpcRemoveModifier(
        this PlayerControl target,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        var modifier = target.GetModifier(typeId, predicate);
        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Player {target.PlayerId} does not have modifier with type ID {typeId}.");
            return;
        }

        target.RpcRemoveModifier(modifier.UniqueId);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player.
    /// </summary>
    /// <param name="player">The player to remove the modifier from.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    public static void RpcRemoveModifier(
        this PlayerControl player,
        Type type,
        Func<BaseModifier, bool>? predicate = null)
    {
        var id = ModifierManager.GetModifierTypeId(type) ?? throw new InvalidOperationException(
            $"Modifier {type.Name} is not registered.");

        player.RpcRemoveModifier(id, predicate);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player.
    /// </summary>
    /// <param name="player">The player to remove the modifier from.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    public static void RpcRemoveModifier<T>(this PlayerControl player, Func<BaseModifier, bool>? predicate = null)
        where T : BaseModifier
    {
        player.RpcRemoveModifier(typeof(T), predicate);
    }

    /// <summary>
    /// Gets the ModifierComponent for a player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <returns>A ModifierComponent if there is one, null otherwise.</returns>
    public static ModifierComponent GetModifierComponent(this PlayerControl player)
    {
        if (ModifierComponents.TryGetValue(player, out var component))
        {
            return component;
        }

        component = player.GetComponent<ModifierComponent>();
        if (!component)
        {
            throw new InvalidOperationException("ModifierComponent is not attached to the player.");
        }

        ModifierComponents[player] = component;
        return component;
    }

    /// <summary>
    /// Checks if the player has a specific modifier.
    /// </summary>
    /// <typeparam name="T">The type of the modifier.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>True if the player has the modifier, false otherwise.</returns>
    public static bool HasModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return player.GetModifierComponent().HasModifier(predicate);
    }

    /// <summary>
    /// Checks if the player has a specific modifier by type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>True if the player has the modifier, false otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, Type type, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().HasModifier(type, predicate);
    }

    /// <summary>
    /// Checks if the player has a specific modifier by type ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="typeId">The type ID of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>True if the player has the modifier, false otherwise.</returns>
    public static bool HasModifier(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().HasModifier(typeId, predicate);
    }

    /// <summary>
    /// Checks if the player has a specific modifier by its GUID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="uniqueId">The unique ID of the modifier.</param>
    /// <returns>True if the player has the modifier, false otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, Guid uniqueId)
    {
        return player.GetModifierComponent().HasModifier(uniqueId);
    }

    /// <summary>
    /// Clears all modifiers from a player.
    /// </summary>
    /// <param name="plr">The player you want to clear modifiers for.</param>
    public static void ClearModifiers(this PlayerControl plr)
    {
        foreach (var mod in plr.GetModifierComponent().ActiveModifiers)
        {
            plr.RemoveModifier(mod);
        }
    }

    /// <summary>
    /// Clears all modifiers from a player.
    /// </summary>
    /// <param name="plr">The player you want to clear modifiers for.</param>
    public static void RpcClearModifiers(this PlayerControl plr)
    {
        foreach (var mod in plr.GetModifierComponent().ActiveModifiers)
        {
            plr.RpcRemoveModifier(mod.UniqueId);
        }
    }

    /// <summary>
    /// Tries to get a modifier by its type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="modifier">The modifier or null.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    public static bool TryGetModifier<T>(this PlayerControl player, [NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return player.GetModifierComponent().TryGetModifier(out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a modifier by its type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The modifier type.</param>
    /// <param name="modifier">The modifier or null.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, Type type, [NotNullWhen(true)] out BaseModifier? modifier, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryGetModifier(type, out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a modifier by its type ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="id">The modifier type ID.</param>
    /// <param name="modifier">The modifier or null.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, uint id, [NotNullWhen(true)] out BaseModifier? modifier, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryGetModifier(id, out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a modifier by its unique ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="modifierGuid">The modifier unique ID.</param>
    /// <param name="modifier">The modifier or null.</param>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, Guid modifierGuid, [NotNullWhen(true)] out BaseModifier? modifier)
    {
        return player.GetModifierComponent().TryGetModifier(modifierGuid, out modifier);
    }

    /// <summary>
    /// Gets a specific modifier from the player.
    /// </summary>
    /// <typeparam name="T">The type of the modifier.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>The modifier if found, null otherwise.</returns>
    public static T? GetModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return player.GetModifierComponent().GetModifier(predicate);
    }

    /// <summary>
    /// Gets a specific modifier from the player by type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>The modifier if found, null otherwise.</returns>
    public static BaseModifier? GetModifier(
        this PlayerControl player,
        Type type,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifier(type, predicate);
    }

    /// <summary>
    /// Gets a specific modifier from the player by its type ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="typeId">The type ID of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>The modifier if found, null otherwise.</returns>
    public static BaseModifier? GetModifier(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifier(typeId, predicate);
    }

    /// <summary>
    /// Gets a specific modifier from the player by its GUID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="uniqueId">The GUID of the modifier.</param>
    /// <returns>The modifier if found, null otherwise.</returns>
    public static BaseModifier? GetModifier(
        this PlayerControl player,
        Guid uniqueId)
    {
        return player.GetModifierComponent().GetModifier(uniqueId);
    }

    /// <summary>
    /// Gets all modifiers of a specific type from the player.
    /// </summary>
    /// <typeparam name="T">The type of the modifiers.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>A collection of modifiers.</returns>
    public static IEnumerable<T> GetModifiers<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : BaseModifier
    {
        return player.GetModifierComponent().GetModifiers(predicate);
    }

    /// <summary>
    /// Gets all modifiers of a specific type from the player.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifiers.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>A collection of modifiers.</returns>
    public static IEnumerable<BaseModifier> GetModifiers(
        this PlayerControl player,
        Type type,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifiers(type, predicate);
    }

    /// <summary>
    /// Gets all modifiers of a specific type from the player by type ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="typeId">The type ID of the modifiers.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>A collection of modifiers.</returns>
    public static IEnumerable<BaseModifier> GetModifiers(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifiers(typeId, predicate);
    }

    /// <summary>
    /// Removes a specific modifier from the player.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="modifier">The modifier to remove.</param>
    public static void RemoveModifier(this PlayerControl player, BaseModifier modifier)
    {
        player.GetModifierComponent().RemoveModifier(modifier);
    }

    /// <summary>
    /// Removes a specific modifier from the player.
    /// </summary>
    /// <typeparam name="T">The type of the modifier.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    public static void RemoveModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : BaseModifier
    {
        player.GetModifierComponent().RemoveModifier(predicate);
    }

    /// <summary>
    /// Removes a specific modifier from the player by type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    public static void RemoveModifier(this PlayerControl player, Type type, Func<BaseModifier, bool>? predicate = null)
    {
        player.GetModifierComponent().RemoveModifier(type, predicate);
    }

    /// <summary>
    /// Removes a specific modifier from the player by its type ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="typeId">The type ID of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    public static void RemoveModifier(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        player.GetModifierComponent().RemoveModifier(typeId, predicate);
    }

    /// <summary>
    /// Removes a specific modifier from the player by its GUID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="uniqueId">The GUID of the modifier.</param>
    public static void RemoveModifier(
        this PlayerControl player,
        Guid uniqueId)
    {
        player.GetModifierComponent().RemoveModifier(uniqueId);
    }

    /// <summary>
    /// Adds a specific modifier to the player.
    /// </summary>
    /// <typeparam name="T">The type of the modifier.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <returns>The added modifier.</returns>
    public static T? AddModifier<T>(this PlayerControl player, params object[] args) where T : BaseModifier
    {
        return player.GetModifierComponent().AddModifier<T>(args);
    }

    /// <summary>
    /// Adds a specific modifier to the player.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>The added modifier.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, BaseModifier modifier)
    {
        return player.GetModifierComponent().AddModifier(modifier);
    }

    /// <summary>
    /// Adds a specific modifier to the player by type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <returns>The added modifier.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, Type type, params object[] args)
    {
        return player.GetModifierComponent().AddModifier(type, args);
    }

    /// <summary>
    /// Adds a specific modifier to the player by type ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="typeId">The type ID of the modifier.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <returns>The added modifier.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, uint typeId, params object[] args)
    {
        return player.GetModifierComponent().AddModifier(typeId, args);
    }
}
