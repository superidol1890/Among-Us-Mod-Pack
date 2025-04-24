using HarmonyLib;
using LaunchpadReloaded.Modifiers.Fun;
using LaunchpadReloaded.Modifiers.Game;
using MiraAPI.Modifiers;

namespace LaunchpadReloaded.Patches.Modifiers;

[HarmonyPatch(typeof(MedScanMinigame))]
public static class MedscanPatch
{
    [HarmonyPatch(nameof(MedScanMinigame.Begin))]
    [HarmonyPostfix]
    public static void OverrideSizePatch(MedScanMinigame __instance)
    {
        if (PlayerControl.LocalPlayer.HasModifier<GiantModifier>())
        {
            __instance.completeString = __instance.completeString.Replace("3' 6\"", "5' 8\"").Replace("92lb", "184lb");
        }


        if (PlayerControl.LocalPlayer.HasModifier<SmolModifier>())
        {
            __instance.completeString = __instance.completeString.Replace("3' 6\"", "1' 8\"").Replace("92lb", "46lb");
        }
    }
}