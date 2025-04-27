using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ResetMoveState))]
    public class ResetMoveState
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (!__instance.myPlayer.IsGhostRole()) return;

            var role = GhostRole.GetGhostRole(__instance.myPlayer);
            __instance.myPlayer.Collider.enabled = !role.Caught;
        }
    }
}