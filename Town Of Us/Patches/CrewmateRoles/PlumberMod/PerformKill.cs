using HarmonyLib;
using Reactor.Utilities;
using System;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.PlumberMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != HudManager.Instance.KillButton) return true;
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Plumber);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!__instance.enabled || __instance.isCoolingDown) return false;
            var role = Role.GetRole<Plumber>(PlayerControl.LocalPlayer);
            if (role.FlushTimer() > 0f || role.Vent == null || role.UsesLeft == 0) return false;
            var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
            if (!abilityUsed) return false;

            PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"{role.Vent.Id}");
            role.UsesLeft--;
            if (!role.FutureBlocks.Contains((byte)role.Vent.Id)) role.FutureBlocks.Add((byte)role.Vent.Id);
            role.LastFlushed = DateTime.UtcNow;
            Utils.Rpc(CustomRPC.Flush, (byte)1, role.Player.PlayerId, (byte)role.Vent.Id);
            return false;
        }
    }
}