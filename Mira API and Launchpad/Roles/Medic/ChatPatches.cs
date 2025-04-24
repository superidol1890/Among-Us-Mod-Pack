using HarmonyLib;
using LaunchpadReloaded.Features;
using LaunchpadReloaded.Modifiers;
using MiraAPI.Modifiers;
using UnityEngine;

namespace LaunchpadReloaded.Patches.Roles.Medic;

/// <summary>
/// Disable chat if revived
/// </summary>
[HarmonyPatch(typeof(ChatController))]
public static class ChatPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ChatController.Update))]
    public static void UpdatePatch(ChatController __instance)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.GetModifierComponent()) return;

        if (PlayerControl.LocalPlayer?.HasModifier<RevivedModifier>() == true)
        {
            __instance.sendRateMessageText.gameObject.SetActive(true);
            __instance.sendRateMessageText.text = "You have been revived. You can no longer speak.";
            __instance.sendRateMessageText.color = LaunchpadPalette.MedicColor;
            __instance.quickChatButton.gameObject.SetActive(false);
            __instance.freeChatField.textArea.gameObject.SetActive(false);
            __instance.openKeyboardButton.gameObject.SetActive(false);
        }
        else
        {
            __instance.sendRateMessageText.color = Color.red;
            __instance.quickChatButton.gameObject.SetActive(true);
            __instance.freeChatField.textArea.gameObject.SetActive(true);
            __instance.openKeyboardButton.gameObject.SetActive(true);
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ChatController.SendChat))]
    public static bool SendChatPatch()
    {
        return !PlayerControl.LocalPlayer.HasModifier<RevivedModifier>();
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ChatController.AddChat))]
    public static bool AddChatPatch([HarmonyArgument(0)] PlayerControl player)
    {
        return !player.HasModifier<RevivedModifier>();
    }
}