using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Events;

/// <summary>
/// General patches for the KillAnimation class.
/// </summary>
[HarmonyPatch]
public static class KillAnimationPatches
{
    /// <summary>
    /// Used to trigger the <see cref="AfterMurderEvent"/>.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(KillAnimation._CoPerformKill_d__2), nameof(KillAnimation._CoPerformKill_d__2.MoveNext))]
    public static void KillAnimPostfix(KillAnimation._CoPerformKill_d__2 __instance)
    {
        // Checking if state is -1 to wait for the end of coroutine.
        if (__instance.__1__state != -1)
        {
            return;
        }

        var afterMurderEvent = new AfterMurderEvent(__instance.source, __instance.target, Helpers.GetBodyById(__instance.target.PlayerId));
        MiraEventManager.InvokeEvent(afterMurderEvent);
    }
}
