using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;

namespace MiraAPI.Patches.Events;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
internal static class EndGamePatches
{
    public static bool Prefix(EndGameManager __instance)
    {
        if (CustomGameOver.Instance is not { } gameOver)
        {
            return true;
        }

        var result = gameOver.BeforeEndGameSetup(__instance);
        return result;
    }

    public static void Postfix(EndGameManager __instance)
    {
        if (CustomGameOver.Instance is { } gameOver)
        {
            gameOver.AfterEndGameSetup(__instance);
            CustomGameOver.Instance = null;
        }

        var @event = new GameEndEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
    }
}
