using System;
using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.OracleMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Oracle);
            if (!flag) return true;
            var role = Role.GetRole<Oracle>(PlayerControl.LocalPlayer);
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (!__instance.enabled) return false;
            if (__instance == role.BlessButton)
            {
                if (!__instance.isActiveAndEnabled || role.ClosestBlessedPlayer == null) return false;
                if (__instance.isCoolingDown) return false;
                if (role.BlessTimer() != 0) return false;

                var interact2 = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestBlessedPlayer);
                if (interact2[4] == true)
                {
                    role.Blessed = role.ClosestBlessedPlayer;
                    Utils.Rpc(CustomRPC.Bless, PlayerControl.LocalPlayer.PlayerId, (byte)1, role.Blessed.PlayerId);
                    role.LastBlessed = DateTime.UtcNow;
                    role.LastBlessed = role.LastBlessed.AddSeconds(1f - CustomGameOptions.BlessCd);
                }
                return false;
            }
            if (role.ClosestPlayer == null) return false;
            if (__instance != HudManager.Instance.KillButton) return true;
            var flag2 = role.ConfessTimer() == 0f;
            if (!flag2) return false;

            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
            if (interact[4] == true)
            {
                role.Confessor = role.ClosestPlayer;
                bool showsCorrectFaction = true;
                int faction = 1;
                if (role.Accuracy == 0f) showsCorrectFaction = false;
                else
                {
                    var num = UnityEngine.Random.RandomRangeInt(1, 101);
                    showsCorrectFaction = num <= role.Accuracy;
                }
                if (showsCorrectFaction)
                {
                    if (role.Confessor.Is(Faction.Crewmates)) faction = 0;
                    else if (role.Confessor.Is(Faction.Impostors)) faction = 2;
                }
                else
                {
                    var num = UnityEngine.Random.RandomRangeInt(0, 2);
                    if (role.Confessor.Is(Faction.Impostors)) faction = num;
                    else if (role.Confessor.Is(Faction.Crewmates)) faction = num + 1;
                    else if (num == 1) faction = 2;
                    else faction = 0;
                }
                if (faction == 0) role.RevealedFaction = Faction.Crewmates;
                else if (faction == 1) role.RevealedFaction = Faction.NeutralEvil;
                else role.RevealedFaction = Faction.Impostors;
                Utils.Rpc(CustomRPC.Confess, PlayerControl.LocalPlayer.PlayerId, role.Confessor.PlayerId, faction);
            }
            if (interact[0] == true)
            {
                role.LastConfessed = DateTime.UtcNow;
                return false;
            }
            else if (interact[1] == true)
            {
                role.LastConfessed = DateTime.UtcNow;
                role.LastConfessed = role.LastConfessed.AddSeconds(CustomGameOptions.TempSaveCdReset - CustomGameOptions.ConfessCd);
                return false;
            }
            else if (interact[3] == true) return false;
            return false;
        }
    }
}
