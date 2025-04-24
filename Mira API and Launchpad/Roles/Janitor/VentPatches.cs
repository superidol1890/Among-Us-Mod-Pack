using HarmonyLib;
using LaunchpadReloaded.Components;

namespace LaunchpadReloaded.Patches.Roles.Janitor;

[HarmonyPatch(typeof(Vent))]
public static class VentPatches
{

    /// <summary>
    /// Expose hidden bodies when venting
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Vent.EnterVent))]
    [HarmonyPatch(nameof(Vent.ExitVent))]
    public static void EnterExitPostfix(Vent __instance)
    {
        var ventBody = __instance.GetComponent<VentBodyComponent>();
        if (ventBody && ventBody.deadBody)
        {
            ventBody.ExposeBody();
        }
    }
}