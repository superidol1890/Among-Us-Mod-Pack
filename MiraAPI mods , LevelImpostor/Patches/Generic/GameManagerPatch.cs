using HarmonyLib;
using LaunchpadReloaded.Components;

namespace LaunchpadReloaded.Patches.Generic;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
public static class GameManagerPatch
{
    public static void Postfix(GameManager __instance)
    {
        __instance.DeadBodyPrefab.gameObject.AddComponent<DeadBodyCacheComponent>();
    }
}