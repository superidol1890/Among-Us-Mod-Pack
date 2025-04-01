using HarmonyLib;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Options;

namespace Lotus.Patches.Systems.Countdown;

[HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Deteriorate))]
public static class DeteriorateReactorPatch
{
    private static bool Initiated = false;
    public static void Prefix(ReactorSystemType __instance)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Reactor)
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
            case ShipStatus.MapType.Ship when GeneralOptions.SabotageOptions.CustomSkeldReactorCountdown:
                __instance.Countdown = GeneralOptions.SabotageOptions.SkeldReactorCountdown;
                break;
            case ShipStatus.MapType.Hq when GeneralOptions.SabotageOptions.CustomMiraReactorCountdown:
                __instance.Countdown = GeneralOptions.SabotageOptions.MiraReactorCountdown;
                break;
            case ShipStatus.MapType.Pb when GeneralOptions.SabotageOptions.CustomPolusReactorCountdown:
                __instance.Countdown = GeneralOptions.SabotageOptions.PolusReactorCountdown;
                break;
        }
    }
}