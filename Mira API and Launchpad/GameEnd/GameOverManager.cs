using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Reactor.Utilities;

namespace MiraAPI.GameEnd;

/// <summary>
/// Manage custom game overs.
/// </summary>
public static class GameOverManager
{
    private static readonly Dictionary<Type, int> GameOverIds = [];
    private static readonly Dictionary<int, Type> GameOverTypes = [];

    private static int _nextId = Enum.GetNames<GameOverReason>().Length;

    /// <summary>
    /// Register a custom game over.
    /// </summary>
    /// <param name="gameOverType">Type of the custom game over.</param>
    /// <exception cref="ArgumentException">Thrown when the type is not a subclass of CustomGameOver, is abstract, or does not have a parameterless constructor.</exception>
    /// <returns>>True if the game over was registered successfully, false otherwise.</returns>
    public static bool RegisterGameOver(Type gameOverType)
    {
        if (!typeof(CustomGameOver).IsAssignableFrom(gameOverType))
        {
            return false;
        }

        if (gameOverType.IsAbstract)
        {
            Logger<MiraApiPlugin>.Error("The type must not be abstract.");
            return false;
        }

        if (gameOverType.GetConstructor(Type.EmptyTypes) == null)
        {
            Logger<MiraApiPlugin>.Error("The type must have a parameterless constructor.");
            return false;
        }

        GameOverIds.Add(gameOverType, _nextId);
        GameOverTypes.Add(_nextId, gameOverType);
        _nextId++;
        return true;
    }

    /// <summary>
    /// Create an instance of a custom game over.
    /// </summary>
    /// <param name="id">ID of the custom game over.</param>
    /// <param name="customGameOver">The created instance of the custom game over.</param>
    /// <returns>An instance of the custom game over.</returns>
    public static bool TryGetGameOver(int id, [NotNullWhen(true)] out CustomGameOver? customGameOver)
    {
        if (GameOverTypes.TryGetValue(id, out var gameOverType))
        {
            customGameOver = Activator.CreateInstance(gameOverType) as CustomGameOver;
            return customGameOver != null;
        }

        customGameOver = null;
        return false;
    }

    /// <summary>
    /// Get the ID of a custom game over.
    /// </summary>
    /// <typeparam name="T">Type of the custom game over.</typeparam>
    /// <returns>The ID of the custom game over.</returns>
    public static int GetGameOverId<T>() where T : CustomGameOver
    {
        return GetGameOverId(typeof(T));
    }

    /// <summary>
    /// Get the ID of a custom game over.
    /// </summary>
    /// <param name="gameOverType">Type of the custom game over.</param>
    /// <returns>The ID of the custom game over.</returns>
    /// <exception cref="ArgumentException">Thrown when the type is not registered.</exception>
    public static int GetGameOverId(Type gameOverType)
    {
        if (GameOverIds.TryGetValue(gameOverType, out var id))
            return id;

        throw new ArgumentException($"{gameOverType.FullName} is not a registered custom game over!");
    }
}
