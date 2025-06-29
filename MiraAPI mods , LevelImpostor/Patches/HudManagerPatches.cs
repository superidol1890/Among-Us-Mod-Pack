using HarmonyLib;
using MiraAPI.Hud;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches;

/// <summary>
/// General patches for the HudManager class.
/// </summary>
[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatches
{
    // Custom buttons parent.
    public static GameObject? BottomLeft { get; private set; }
    public static Transform? BottomRight { get; private set; }
    public static Transform? Buttons { get; private set; }

    /*
    /// <summary>
    /// Trigger hudstart on current custom gamemode
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.OnGameStart))]
    public static void GameStartPatch(HudManager __instance)
    {
        CustomGameModeManager.ActiveMode?.HudStart(__instance);
    }*/

    /// <summary>
    /// Create custom buttons and arrange them on the hud.
    /// </summary>
    /// <param name="__instance">The HudManager instance.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Start))]
    public static void StartPostfix(HudManager __instance)
    {
        if (Buttons == null)
        {
            Buttons = __instance.transform.Find("Buttons");
        }

        if (BottomRight == null)
        {
            BottomRight = Buttons.Find("BottomRight");
        }

        if (BottomLeft == null)
        {
            BottomLeft = Object.Instantiate(BottomRight.gameObject, Buttons);
        }

        foreach (var t in BottomLeft.GetComponentsInChildren<ActionButton>(true))
        {
            t.gameObject.Destroy();
        }

        var gridArrange = BottomLeft.GetComponent<GridArrange>();
        var aspectPosition = BottomLeft.GetComponent<AspectPosition>();

        BottomLeft.name = "BottomLeft";
        gridArrange.Alignment = GridArrange.StartAlign.Right;
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            var location = button.Location switch
            {
                ButtonLocation.BottomLeft => BottomLeft.transform,
                ButtonLocation.BottomRight => BottomRight,
                _ => null,
            };

            if (location is null)
            {
                continue;
            }

            button.CreateButton(location);
        }

        gridArrange.Start();
        gridArrange.ArrangeChilds();

        aspectPosition.AdjustPosition();
    }

    /// <summary>
    /// Set the custom buttons active when the hud is active.
    /// </summary>
    /// <param name="__instance">HudManager instance.</param>
    /// <param name="localPlayer">The local PlayerControl.</param>
    /// <param name="role">The player's RoleBehaviour.</param>
    /// <param name="isActive">Whether the Hud should be set active or not.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
    public static void SetHudActivePostfix(HudManager __instance, PlayerControl localPlayer, RoleBehaviour role, bool isActive)
    {
        __instance.AdminButton.ToggleVisible(isActive && role.IsImpostor && GameOptionsManager.Instance.CurrentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek);
        if (localPlayer.Data == null)
        {
            return;
        }

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            button.SetActive(isActive, role);
        }
    }
}
