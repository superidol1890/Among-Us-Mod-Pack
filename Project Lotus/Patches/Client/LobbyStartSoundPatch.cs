using HarmonyLib;

namespace Lotus.Patches.Client;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]

public class StopLobbyStartSoundPatch
{
    public static void Prefix(GameStartManager __instance) => SoundManager.Instance.StopSound(__instance.gameStartSound);
}