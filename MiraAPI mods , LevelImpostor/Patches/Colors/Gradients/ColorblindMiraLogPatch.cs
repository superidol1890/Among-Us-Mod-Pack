using System;
using AmongUs.Data;
using HarmonyLib;
using LaunchpadReloaded.Features.Managers;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(LogEntryBubble),nameof(LogEntryBubble.SetText))]
public static class ColorblindMiraLogPatch
{
    public static void Postfix(LogEntryBubble __instance, [HarmonyArgument(1)] NetworkedPlayerInfo playerData)
    {
        var colorId = playerData.DefaultOutfit.ColorId;
        if (!DataManager.Settings.Accessibility.ColorBlindMode)
        {
            return;
        }
        
        var text2 = DestroyableSingleton<TranslationController>.Instance.GetString(Palette.ColorNames[colorId]).ToLower();
        if (!GradientManager.TryGetColor(playerData.PlayerId, out var color))
        {
            return;
        }
        
        var txt = __instance.text.text; 
        var i = txt.LastIndexOf(text2, StringComparison.OrdinalIgnoreCase);
        __instance.text.text = txt.Insert(i, $"{Palette.GetColorName(color).ToLower()}-");
    }
}