using HarmonyLib;
using InnerNet;
using LaunchpadReloaded.Buttons.Crewmate;
using LaunchpadReloaded.Features;
using LaunchpadReloaded.Utilities;
using MiraAPI.Hud;

namespace LaunchpadReloaded.Patches.Generic;

[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatches
{
    /// <summary>
    /// Create Notepad button and screen
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Start))]
    public static void StartPostfix(HudManager __instance)
    {
        if (NotepadHud.Instance != null)
        {
            NotepadHud.Instance.Destroy();
        }

        new NotepadHud(__instance);
    }

    /// <summary>
    /// Generic update method for most of HUD logic in Launchpad
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Update))]
    public static void UpdatePostfix(HudManager __instance)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || MeetingHud.Instance)
        {
            if (CustomButtonSingleton<ZoomButton>.Instance.EffectActive)
            {
                CustomButtonSingleton<ZoomButton>.Instance.OnEffectEnd();
            }

            return;
        }

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !ShipStatus.Instance)
        {
            return;
        }

        if (HackerUtilities.AnyPlayerHacked())
        {
            __instance.ReportButton.SetActive(false);
        }
    }
}