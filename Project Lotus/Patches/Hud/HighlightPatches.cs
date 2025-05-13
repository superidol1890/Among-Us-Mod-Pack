using HarmonyLib;
using Lotus.Extensions;
using Lotus.Roles;
using UnityEngine;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches.Hud;

public class HighlightPatches
{
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int AddColor = Shader.PropertyToID("_AddColor");

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.ToggleHighlight))]
    public static void TogglePlayerHighlight(PlayerControl __instance, RoleTeamTypes targeterTeam)
    {
        var player = PlayerControl.LocalPlayer;
        if (player.Data.IsDead) return;
        __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor(OutlineColor, player.PrimaryRole().RoleColor);
    }

    [QuickPostfix(typeof(Vent), nameof(Vent.SetOutline))]
    public static void SetVentOutline(Vent __instance, [HarmonyArgument("mainTarget")] ref bool mainTarget)
    {
        CustomRole role = PlayerControl.LocalPlayer.PrimaryRole();

        __instance.myRend.material.SetColor(OutlineColor, role.RoleColor);
        __instance.myRend.material.SetColor(AddColor, mainTarget ? role.RoleColor : Color.clear);
    }
}