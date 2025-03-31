using HarmonyLib;
using TownOfHost.Roles.Core;

namespace TownOfHost;

[HarmonyPatch(typeof(OneWayShadows), nameof(OneWayShadows.IsIgnored))]
public static class OneWayShadowsIsIgnoredPatch
{
    public static bool Prefix(OneWayShadows __instance, ref bool __result)
    {
        var roleInfo = PlayerControl.LocalPlayer.GetCustomRole().GetRoleInfo();
        var amDesyncImpostor = roleInfo?.IsDesyncImpostor == true;
        if (__instance.IgnoreImpostor && amDesyncImpostor)
        {
            __result = true;
            return false;
        }
        return true;
    }
}
