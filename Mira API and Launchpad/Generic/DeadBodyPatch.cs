using HarmonyLib;

namespace LaunchpadReloaded.Patches.Generic;

[HarmonyPatch(typeof(DeadBody))]
public static class DeadBodyPatch
{
    /// <summary>
    /// This is so the report button doesn't light up after reporting. (For Coroner freeze ability)
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(DeadBody.OnClick))]
    public static void ClickBodyPatch(DeadBody __instance)
    {
        if (__instance.Reported)
        {
            __instance.enabled = false;
        }
    }
}