using HarmonyLib;
using LaunchpadReloaded.API.Settings;
using LaunchpadReloaded.Utilities;

namespace LaunchpadReloaded.Patches.Settings;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    public static void StartPostfix(OptionsMenuBehaviour __instance)
    {
        CustomSettingsManager.CreateTab(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.ToggleColorBlind))]
    public static void ChangeColorblindPatch(OptionsMenuBehaviour __instance)
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }

        foreach (var plr in PlayerControl.AllPlayerControls)
        {
            var playerTagManager = plr.GetTagManager();
            if (playerTagManager != null)
            {
                playerTagManager.UpdatePosition();
            }
        }
    }
}