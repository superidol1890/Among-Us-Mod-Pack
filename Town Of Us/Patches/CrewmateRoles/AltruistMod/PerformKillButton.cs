using HarmonyLib;
using Reactor.Utilities;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.AltruistMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKillButton
    {
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != HudManager.Instance.KillButton) return true;
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Altruist);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Altruist>(PlayerControl.LocalPlayer);
            if (!role.ButtonUsable || role.CurrentlyReviving) return false;
            DeadBody closestBody = HudManagerUpdate.GetClosestBody(PlayerControl.LocalPlayer);
            bool canRevive = closestBody == null ? false : Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), closestBody.transform.localPosition)
                <= CustomGameOptions.ReviveRadius * ShipStatus.Instance.MaxLightRadius;
            if (!canRevive) return false;
            var reviveButton = HudManager.Instance.KillButton;
            if (__instance == reviveButton)
            {
                if (__instance.isCoolingDown) return false;
                if (!__instance.isActiveAndEnabled) return false;
                var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                if (!abilityUsed) return false;
                Coroutines.Start(Coroutine.AltruistRevive(role));
                Utils.Rpc(CustomRPC.AltruistRevive, PlayerControl.LocalPlayer.PlayerId, (byte)0);
                return false;
            }

            return true;
        }
    }
}