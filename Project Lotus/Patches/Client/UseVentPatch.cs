using HarmonyLib;
using UnityEngine;
using Lotus.Roles;
using Lotus.Extensions;
using AmongUs.GameOptions;
using static AmongUs.GameOptions.GameModes;
using Lotus.Logging;
using System.Linq;
using Lotus.Roles.Subroles;

namespace Lotus.Patches.Client;

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
class UseVentPatch
{
    public static bool CanUseVent(PlayerControl player, CustomRole role, Vent _, bool isSubRole = false)
    {
        if (isSubRole) return role.CanVent();
        bool canUseVent = player.GetSubroles().Count != 0 ? player.GetSubroles().Any(sR => CanUseVent(player, sR, _, true)) : false;
        RoleTypes vanillaRole = player.Data.Role.Role;
        if (vanillaRole.IsCrewmate()) canUseVent = canUseVent || vanillaRole is RoleTypes.Engineer;
        canUseVent = canUseVent || role.CanVent();
        return canUseVent;
    }

    // THANKS TOWN OF US.
    public static void Postfix(Vent __instance,
            [HarmonyArgument(0)] NetworkedPlayerInfo playerInfo,
            [HarmonyArgument(1)] ref bool canUse,
            [HarmonyArgument(2)] ref bool couldUse,
            ref float __result)
    {
        float num = float.MaxValue;
        PlayerControl playerControl = playerInfo.Object;

        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == Normal) couldUse = CanUseVent(playerControl, playerControl.PrimaryRole(), __instance) && !playerControl.MustCleanVent(__instance.Id) && (!playerInfo.IsDead || playerControl.inVent) && (playerControl.CanMove || playerControl.inVent);
        else if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == HideNSeek && playerControl.GetVanillaRole().IsImpostor()) couldUse = false;
        else couldUse = canUse;

        var ventitaltionSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].Cast<VentilationSystem>();

        if (ventitaltionSystem != null && ventitaltionSystem.IsVentCurrentlyBeingCleaned(__instance.Id))
        {
            couldUse = false;
        }

        canUse = couldUse;

        if (canUse)
        {
            Vector3 center = playerControl.Collider.bounds.center;
            Vector3 position = __instance.transform.position;
            num = Vector2.Distance((Vector2)center, (Vector2)position);

            canUse = ((canUse ? 1 : 0) & ((double)num > (double)__instance.UsableDistance ? 0 : (!PhysicsHelpers.AnythingBetween(playerControl.Collider, (Vector2)center, (Vector2)position, Constants.ShipOnlyMask, false) ? 1 : 0))) != 0;
            // canUse &= num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(playerControl.Collider, center, position, Constants.ShipOnlyMask, false);
        }

        __result = num;
    }
}