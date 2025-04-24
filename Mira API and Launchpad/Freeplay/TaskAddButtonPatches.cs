using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches.Freeplay;

[HarmonyPatch(typeof(TaskAddButton))]
internal static class TaskAddButtonPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TaskAddButton.Start))]
    public static bool StartPrefix(TaskAddButton __instance)
    {
        // if this becomes problematic in the future, find a new method.
        if (uint.TryParse(__instance.name, out var result))
        {
            __instance.Overlay.sprite = __instance.CheckImage;
            __instance.Overlay.enabled = PlayerControl.LocalPlayer.HasModifier(result);
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TaskAddButton.Role), MethodType.Setter)]
    public static void RoleGetterPatch(TaskAddButton __instance)
    {
        if (__instance.role is ICustomRole { Team: ModdedRoleTeams.Custom } customRole)
        {
            __instance.FileImage.color = customRole.IntroConfiguration?.IntroTeamColor ?? Color.gray;
        }
        __instance.RolloverHandler.OutColor = __instance.FileImage.color;
    }
}
