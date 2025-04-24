using HarmonyLib;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Patches the haunt menu to show the actual role name rather then team.
/// </summary>
[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
public static class HauntMenuMinigamePatch
{
    public static void Postfix(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget.Data.IsDead)
        {
            __instance.FilterText.color = Color.white;
            return;
        }

        var role = __instance.HauntTarget.Data.Role;
        var color = role is ICustomRole custom ? custom.RoleColor : role.TeamColor;

        __instance.FilterText.text = role.NiceName;
        __instance.FilterText.color = color;
    }
}