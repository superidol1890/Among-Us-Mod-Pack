using HarmonyLib;
using TownOfUs.Roles;
using TownOfUs.Extensions;
using UnityEngine;
using System;

namespace TownOfUs.ImpostorRoles.BlackmailerMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Blackmailer)) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Blackmailer>(PlayerControl.LocalPlayer);
            var target = role.ClosestPlayer;
            if (__instance == role.BlackmailButton)
            {
                if (role.Player.inVent) return false;
                if (!__instance.isActiveAndEnabled || role.ClosestPlayer == null) return false;
                if (__instance.isCoolingDown) return false;
                if (!__instance.isActiveAndEnabled) return false;
                if (role.BlackmailTimer() != 0) return false;

                var interact = Utils.Interact(PlayerControl.LocalPlayer, target);
                if (interact[4] == true)
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        role.Blackmailed?.myRend().material.SetFloat("_Outline", 0f);
                        if (role.Blackmailed != null && role.Blackmailed.Data.IsImpostor())
                        {
                            if (role.Blackmailed.GetCustomOutfitType() != CustomPlayerOutfitType.Camouflage &&
                                role.Blackmailed.GetCustomOutfitType() != CustomPlayerOutfitType.Swooper)
                                role.Blackmailed.nameText().color = Patches.Colors.Impostor;
                            else role.Blackmailed.nameText().color = Color.clear;
                        }
                        role.Blackmailed = target;
                        Utils.Rpc(CustomRPC.Blackmail, PlayerControl.LocalPlayer.PlayerId, target.PlayerId, (byte)1);
                    }
                    else Utils.Rpc(CustomRPC.Blackmail, PlayerControl.LocalPlayer.PlayerId, target.PlayerId, (byte)0);
                }
                role.BlackmailButton.SetCoolDown(1f, 10f);
                role.LastBlackmailed = DateTime.UtcNow;
                role.LastBlackmailed = role.LastBlackmailed.AddSeconds(1f - CustomGameOptions.BlackmailCd);
                return false;
            }
            return true;
        }
    }
}