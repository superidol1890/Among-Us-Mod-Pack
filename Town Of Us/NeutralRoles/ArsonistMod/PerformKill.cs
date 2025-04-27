using System;
using HarmonyLib;
using TownOfUs.Roles;
using AmongUs.GameOptions;

namespace TownOfUs.NeutralRoles.ArsonistMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Arsonist);
            if (!flag) return true;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            var role = Role.GetRole<Arsonist>(PlayerControl.LocalPlayer);
            if (!__instance.isActiveAndEnabled || __instance.isCoolingDown) return false;
            if (role.DouseTimer() > 0) return false;

            if (__instance == role.IgniteButton)
            {
                if (role.CanIgnite)
                {
                    var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                    if (!abilityUsed) return false;
                    role.LastDoused = System.DateTime.UtcNow;
                    role.Ignite();
                }
                return false;
            }

            if (__instance != HudManager.Instance.KillButton) return true;
            if (role.ClosestPlayer == null) return false;
            var distBetweenPlayers = Utils.GetDistBetweenPlayers(PlayerControl.LocalPlayer, role.ClosestPlayer);
            var flag2 = distBetweenPlayers <
                        LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            if (!flag2) return false;
            if (role.DousedPlayers.Contains(role.ClosestPlayer.PlayerId)) return false;
            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
            if (interact[4] == true)
            {
                role.DousedPlayers.Add(role.ClosestPlayer.PlayerId);
                Utils.Rpc(CustomRPC.Douse, PlayerControl.LocalPlayer.PlayerId, role.ClosestPlayer.PlayerId);
            }
            if (interact[0] == true)
            {
                role.LastDoused = DateTime.UtcNow;
                return false;
            }
            else if (interact[1] == true)
            {
                role.LastDoused = DateTime.UtcNow;
                role.LastDoused.AddSeconds(CustomGameOptions.TempSaveCdReset - CustomGameOptions.DouseCd);
                return false;
            }
            else if (interact[3] == true) return false;
            return false;
        }
    }
}
