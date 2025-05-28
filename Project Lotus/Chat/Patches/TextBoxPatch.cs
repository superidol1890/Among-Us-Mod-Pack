using HarmonyLib;
using Lotus.Extensions;
using Lotus.Logging;
using UnityEngine;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Chat.Patches;

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public class TextBoxPatch
{
    public static void Postfix(TextBoxTMP __instance, char i, ref bool __result)
    {
        if (!__instance.gameObject.HasParentInHierarchy("ChatScreenRoot/ChatScreenContainer")) return;
        __result = __result || (i >= 31 && i <= 126);
    }

    [QuickPrefix(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
    public static void ModifyCharacterLimit(TextBoxTMP __instance)
    {
        if (!__instance.gameObject.HasParentInHierarchy("ChatScreenRoot/ChatScreenContainer")) return;
        __instance.characterLimit = AmongUsClient.Instance.AmHost ? 2000 : 300;
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public static class IsCharAllowedPatch
{
    public static bool Prefix(TextBoxTMP __instance, char i, ref bool __result)
    {
        if (!__instance.gameObject.HasParentInHierarchy("ChatScreenRoot/ChatScreenContainer")) return true;
        __result = !(i == '\b');    // Bugfix: '\b' messing with chat message
        return false;
    }

    public static void Postfix(TextBoxTMP __instance)
    {
        if (!__instance.gameObject.HasParentInHierarchy("ChatScreenRoot/ChatScreenContainer")) return;
        __instance.allowAllCharacters = true; // not used by game's code, but I include it anyway
        __instance.AllowEmail = true;
        __instance.AllowSymbols = true;
    }
}