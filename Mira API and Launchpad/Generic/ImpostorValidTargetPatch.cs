using HarmonyLib;
using LaunchpadReloaded.Options;
using MiraAPI.GameOptions;

namespace LaunchpadReloaded.Patches.Generic;

/// <summary>
/// Allow Impostors to kill each other if can kill is enabled in gamemode or friendly fire is toggled on
/// </summary>
[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
public static class ImpostorValidTargetPatch
{
    public static bool Prefix(ImpostorRole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target, ref bool __result)
    {
        if (!OptionGroupSingleton<FunOptions>.Instance.FriendlyFire)
        {
            return true;
        }
        
        __result = target is { Disconnected: false, IsDead: false } &&
                   target.PlayerId != __instance.Player.PlayerId && !(target.Role == null) &&
                   !(target.Object == null) && !target.Object.inVent && !target.Object.inMovingPlat;
        return false;

    }
}