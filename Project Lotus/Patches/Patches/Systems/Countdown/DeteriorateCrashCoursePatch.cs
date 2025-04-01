using HarmonyLib;
using Lotus.Options;

namespace Lotus.Patches.Systems.Countdown;

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Deteriorate))]
public static class DeteriorateCrashCoursePatch
{
    private static bool Initiated = false;
    public static void Prefix(HeliSabotageSystem __instance)
    {
        if (!__instance.IsActive)
        {
            Initiated = false;
            return;
        }
        if (Initiated) return;

        if (!GeneralOptions.SabotageOptions.CustomAirshipReactorCountdown) return;

        __instance.Countdown = GeneralOptions.SabotageOptions.AirshipReactorCountdown;
        Initiated = true;
    }
}