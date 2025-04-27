using HarmonyLib;
using TownOfUs.Roles;
using System;

namespace TownOfUs.NeutralRoles.MercenaryMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class Vest
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Mercenary);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!__instance.isActiveAndEnabled || __instance.isCoolingDown) return false;
            var role = Role.GetRole<Mercenary>(PlayerControl.LocalPlayer);

            if (__instance == role.GuardButton)
            {
                if (role.ClosestGuardPlayer == null || role.UsesLeft == 0) return false;
                var interact2 = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestGuardPlayer);
                if (interact2[4] == true)
                {
                    role.Guarded.Add(role.ClosestGuardPlayer.PlayerId);
                }
                if (interact2[0] == true)
                {
                    role.LastGuarded = DateTime.UtcNow;
                    return false;
                }
                else if (interact2[1] == true)
                {
                    role.LastGuarded = DateTime.UtcNow;
                    role.LastGuarded.AddSeconds(CustomGameOptions.TempSaveCdReset - CustomGameOptions.GuardCd);
                    return false;
                }
                else if (interact2[3] == true) return false;
                return false;
            }

            if (role.Gold < CustomGameOptions.GoldToBribe) return false;
            if (__instance != HudManager.Instance.KillButton) return true;
            if (role.ClosestBribePlayer == null) return false;
            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestBribePlayer);
            if (interact[4] == true)
            {
                role.Bribed.Add(role.ClosestBribePlayer.PlayerId);
                role.Gold -= CustomGameOptions.GoldToBribe;
                Utils.Rpc(CustomRPC.Bribe, PlayerControl.LocalPlayer.PlayerId, role.ClosestBribePlayer.PlayerId);
            }
            return false;
        }
    }
}