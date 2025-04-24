using System.Collections.Generic;
using HarmonyLib;
using LaunchpadReloaded.Features.Managers;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class ShapeshiftPatch
{
    private static Dictionary<byte, byte> ColorCache { get; } = [];
    
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer)
    {
        var targetPlayerInfo = targetPlayer.Data;
        if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
        {
            __instance.RawSetOutfit(__instance.Data.DefaultOutfit, PlayerOutfitType.Default);
            __instance.SetGradient(ColorCache[__instance.PlayerId]);
        }
        else if (GradientManager.TryGetColor(__instance.PlayerId, out var originalColor) &&
            GradientManager.TryGetColor(targetPlayer.PlayerId, out var color))
        {
            ColorCache[__instance.PlayerId] = originalColor;
            __instance.SetGradient(color);
        }
    }
}