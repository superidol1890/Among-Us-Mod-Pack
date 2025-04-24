using System.Collections.Generic;
using MiraAPI.Networking;
using Reactor.Networking.Rpc;

namespace MiraAPI.GameEnd;

/// <summary>
/// Custom Game Over.
/// </summary>
public abstract class CustomGameOver
{
    /// <summary>
    /// Gets the current CustomGameOver.
    /// </summary>
    public static CustomGameOver? Instance { get; internal set; }

    /// <summary>
    /// Verifies if the condition for this CustomGameOver is met.
    /// </summary>
    /// <param name="playerControl">The PlayerControl that requested the Game Over.</param>
    /// <param name="winners">The collection of winners.</param>
    /// <returns>True if the condition is met, otherwise false.</returns>
    public virtual bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return true;
    }

    /// <summary>
    /// Runs before the base game calls EndGameManager.SetEverythingUp.
    /// </summary>
    /// <param name="endGameManager">The EndGameManager instance.</param>
    /// <returns>Return True to use the run the original method, return False to skip it.</returns>
    public virtual bool BeforeEndGameSetup(EndGameManager endGameManager)
    {
        return true;
    }

    /// <summary>
    /// Runs after the base game calls EndGameManager.SetEverythingUp.
    /// </summary>
    /// <param name="endGameManager">The EndGameManager instance.</param>
    public virtual void AfterEndGameSetup(EndGameManager endGameManager)
    {
    }

    /// <summary>
    /// Get the GameOverReason associated with this CustomGameOver.
    /// </summary>
    /// <typeparam name="T">Type of the custom game over.</typeparam>
    /// <returns>The GameOverReason associated with the custom game over.</returns>
    public static GameOverReason GameOverReason<T>() where T : CustomGameOver
    {
        return (GameOverReason)GameOverManager.GetGameOverId<T>();
    }

    /// <summary>
    /// Send a custom game over.
    /// </summary>
    /// <param name="winners">A collection of winners.</param>
    /// <typeparam name="T">Type of the custom game over.</typeparam>
    public static void Trigger<T>(IEnumerable<NetworkedPlayerInfo> winners) where T : CustomGameOver
    {
        var reason = GameOverManager.GetGameOverId<T>();
        var data = new GameOverData(reason, [.. winners]);

        Rpc<CustomGameOverRpc>.Instance.Send(PlayerControl.LocalPlayer, data, true);
    }

    /// <summary>
    /// Implicitly convert a CustomGameOver to a GameOverReason.
    /// </summary>
    /// <param name="gameOver">The CustomGameOver instance.</param>
    /// <returns>The GameOverReason associated with the CustomGameOver.</returns>
    public static implicit operator GameOverReason(CustomGameOver gameOver)
    {
        return (GameOverReason)GameOverManager.GetGameOverId(gameOver.GetType());
    }
}
