using HarmonyLib;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
internal static class MinigameBeginErrorPatch
{
    public static void Prefix(Minigame __instance)
    {
        __instance.logger ??= new Logger(Logger.Category.Gameplay, "Minigame");
    }
}
