using HarmonyLib;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Options;

namespace Lotus.Patches.Systems.Countdown;

[HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.Deteriorate))]
public static class DeteriorateOxygenPatch
{
    private static bool Initiated = false;
    public static void Prefix(LifeSuppSystemType __instance)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Oxygen)
            SabotagePatch.SabotageCountdown = __instance.Countdown;

        if (!__instance.IsActive)
        {
            Initiated = false;
            return;
        }
        if (Initiated) return;
        Initiated = true;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship when GeneralOptions.SabotageOptions.CustomSkeldOxygenCountdown:
                __instance.Countdown = GeneralOptions.SabotageOptions.SkeldOxygenCountdown;
                break;
            case ShipStatus.MapType.Hq when GeneralOptions.SabotageOptions.CustomMiraOxygenCountdown:
                __instance.Countdown = GeneralOptions.SabotageOptions.MiraOxygenCountdown;
                break;
        }
    }
}