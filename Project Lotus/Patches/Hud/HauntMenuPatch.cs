using HarmonyLib;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles.Managers.Interfaces;

namespace Lotus.Patches.Hud;

[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
class HauntMenuPatch
{
    public static bool Prefix(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget == null) return true;
        CustomRole role = __instance.HauntTarget.PrimaryRole();
        if (role.GetType() == IRoleManager.Current.FallbackRole().GetType()) return true;
        __instance.NameText.text = __instance.HauntTarget.name;
        __instance.FilterText.text = role.RoleName;
        return false;
    }
}