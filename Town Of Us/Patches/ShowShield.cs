using HarmonyLib;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.Patches
{
    public enum ShieldOptions
    {
        Self = 0,
        Medic = 1,
        SelfAndMedic = 2
    }

    public enum FortifyOptions
    {
        Self = 0,
        Warden = 1,
        SelfAndWarden = 2
    }

    public enum ProtectOptions
    {
        Self = 0,
        GA = 1,
        SelfAndGA = 2
    }

    public enum BarrierOptions
    {
        Self = 0,
        Cleric = 1,
        SelfAndCleric = 2
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class ShowShield
    {
        public static Color RoundOneShieldColor = Color.green;
        public static Color ProtectedColor = new Color(1f, 0.85f, 0f, 1f);
        public static Color ShieldedColor = Color.cyan;
        public static Color FortifiedColor = new Color(0.85f, 0f, 1f, 1f);
        public static Color BarrieredColor = Color.blue;
        public static string DiedFirst = "";
        public static PlayerControl FirstRoundShielded = null;

        public static void Postfix()
        {
            if (FirstRoundShielded != null && !FirstRoundShielded.Data.Disconnected && !IsConcealed(FirstRoundShielded))
            {
                FirstRoundShielded.myRend().material.SetColor("_VisorColor", RoundOneShieldColor);
                FirstRoundShielded.myRend().material.SetFloat("_Outline", 1f);
                FirstRoundShielded.myRend().material.SetColor("_OutlineColor", RoundOneShieldColor);
            }
            else if (FirstRoundShielded != null && !FirstRoundShielded.Data.Disconnected && IsConcealed(FirstRoundShielded))
            {
                ResetVisor(FirstRoundShielded);
            }

            if (MeetingHud.Instance && FirstRoundShielded != null)
            {
                if (!FirstRoundShielded.Data.Disconnected)
                {
                    ResetVisor(FirstRoundShielded);
                }
                FirstRoundShielded = null;
            }

            var showFortify = CustomGameOptions.ShowFortified;
            var showShield = CustomGameOptions.ShowShielded;
            var showBarrier = CustomGameOptions.ShowBarriered;
            var showProtect = CustomGameOptions.ShowProtect;

            foreach (var role in Role.GetRoles(RoleEnum.Warden))
            {
                var warden = (Warden)role;
                var player = warden.Fortified;
                if (player != null && (player.Data.IsDead || player.Data.Disconnected || warden.Player.Data.IsDead || warden.Player.Data.Disconnected))
                {
                    ResetVisor(player, warden.Player);
                    warden.Fortified = null;
                    continue;
                }
                if (player == FirstRoundShielded || player == null || player.Data.Disconnected) continue;
                if ((PlayerControl.LocalPlayer == player && (showFortify == FortifyOptions.Self || showFortify == FortifyOptions.SelfAndWarden)) ||
                    (PlayerControl.LocalPlayer == warden.Player && (showFortify == FortifyOptions.Warden || showFortify == FortifyOptions.SelfAndWarden)))
                {
                    player.myRend().material.SetColor("_VisorColor", FortifiedColor);
                    player.myRend().material.SetFloat("_Outline", 1f);
                    player.myRend().material.SetColor("_OutlineColor", FortifiedColor);
                }
            }

            foreach (var role in Role.GetRoles(RoleEnum.Medic))
            {
                var medic = (Medic)role;
                var player = medic.ShieldedPlayer;
                if (player != null && (player.Data.IsDead || player.Data.Disconnected || medic.Player.Data.IsDead || medic.Player.Data.Disconnected))
                {
                    ResetVisor(player, medic.Player);
                    medic.ShieldedPlayer = null;
                    continue;
                }
                if (player == FirstRoundShielded || player == null || player.Data.Disconnected) continue;
                if (player.IsFortified() && ((PlayerControl.LocalPlayer == player && (showFortify == FortifyOptions.Self || showFortify == FortifyOptions.SelfAndWarden)) ||
                    (PlayerControl.LocalPlayer.Is(RoleEnum.Warden) && player.GetWarden().Contains(Role.GetRole<Warden>(PlayerControl.LocalPlayer)) && (showFortify == FortifyOptions.Warden || showFortify == FortifyOptions.SelfAndWarden)))) continue;
                if ((PlayerControl.LocalPlayer == player && (showShield == ShieldOptions.Self || showShield == ShieldOptions.SelfAndMedic)) ||
                    (PlayerControl.LocalPlayer == medic.Player && (showShield == ShieldOptions.Medic || showShield == ShieldOptions.SelfAndMedic)))
                {
                    player.myRend().material.SetColor("_VisorColor", ShieldedColor);
                    player.myRend().material.SetFloat("_Outline", 1f);
                    player.myRend().material.SetColor("_OutlineColor", ShieldedColor);
                }
            }

            foreach (var role in Role.GetRoles(RoleEnum.Cleric))
            {
                var cler = (Cleric)role;
                var player = cler.Barriered;
                if (player == FirstRoundShielded || player == null || player.Data.Disconnected) continue;
                if (player.IsFortified() && ((PlayerControl.LocalPlayer == player && (showFortify == FortifyOptions.Self || showFortify == FortifyOptions.SelfAndWarden)) ||
                    (PlayerControl.LocalPlayer.Is(RoleEnum.Warden) && player.GetWarden().Contains(Role.GetRole<Warden>(PlayerControl.LocalPlayer)) && (showFortify == FortifyOptions.Warden || showFortify == FortifyOptions.SelfAndWarden)))) continue;
                if (player.IsShielded() && ((PlayerControl.LocalPlayer == player && (showShield == ShieldOptions.Self || showShield == ShieldOptions.SelfAndMedic)) ||
                    (PlayerControl.LocalPlayer.Is(RoleEnum.Medic) && player.GetMedic().Contains(Role.GetRole<Medic>(PlayerControl.LocalPlayer)) && (showShield == ShieldOptions.Medic || showShield == ShieldOptions.SelfAndMedic)))) continue;
                if ((PlayerControl.LocalPlayer == player && (showBarrier == BarrierOptions.Self || showBarrier == BarrierOptions.SelfAndCleric)) ||
                    (PlayerControl.LocalPlayer == cler.Player && (showBarrier == BarrierOptions.Cleric || showBarrier == BarrierOptions.SelfAndCleric)))
                {
                    player.myRend().material.SetColor("_VisorColor", BarrieredColor);
                    player.myRend().material.SetFloat("_Outline", 1f);
                    player.myRend().material.SetColor("_OutlineColor", BarrieredColor);
                }
            }

            foreach (var role in Role.GetRoles(RoleEnum.GuardianAngel))
            {
                var ga = (GuardianAngel)role;
                var player = ga.target;
                if (player == FirstRoundShielded || player == null || player.Data.Disconnected) continue;
                if (player.IsFortified() && ((PlayerControl.LocalPlayer == player && (showFortify == FortifyOptions.Self || showFortify == FortifyOptions.SelfAndWarden)) ||
                    (PlayerControl.LocalPlayer.Is(RoleEnum.Warden) && player.GetWarden().Contains(Role.GetRole<Warden>(PlayerControl.LocalPlayer)) && (showFortify == FortifyOptions.Warden || showFortify == FortifyOptions.SelfAndWarden)))) continue;
                if (player.IsShielded() && ((PlayerControl.LocalPlayer == player && (showShield == ShieldOptions.Self || showShield == ShieldOptions.SelfAndMedic)) ||
                    (PlayerControl.LocalPlayer.Is(RoleEnum.Medic) && player.GetMedic().Contains(Role.GetRole<Medic>(PlayerControl.LocalPlayer)) && (showShield == ShieldOptions.Medic || showShield == ShieldOptions.SelfAndMedic)))) continue;
                if (player.IsBarriered() && ((PlayerControl.LocalPlayer == player && (showBarrier == BarrierOptions.Self || showBarrier == BarrierOptions.SelfAndCleric)) ||
                    (PlayerControl.LocalPlayer.Is(RoleEnum.Cleric) && player.GetCleric().Contains(Role.GetRole<Cleric>(PlayerControl.LocalPlayer)) && (showBarrier == BarrierOptions.Cleric || showBarrier == BarrierOptions.SelfAndCleric)))) continue;
                if (ga.Protecting)
                {
                    if ((PlayerControl.LocalPlayer == player && (showProtect == ProtectOptions.Self || showProtect == ProtectOptions.SelfAndGA)) ||
                        (PlayerControl.LocalPlayer == ga.Player && (showProtect == ProtectOptions.GA || showProtect == ProtectOptions.SelfAndGA)))
                    {
                        player.myRend().material.SetColor("_VisorColor", ProtectedColor);
                        player.myRend().material.SetFloat("_Outline", 1f);
                        player.myRend().material.SetColor("_OutlineColor", ProtectedColor);
                    }
                }
            }
        }

        public static void ResetVisor(PlayerControl player, PlayerControl saviour = null)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                var saves = 0;
                if (player == FirstRoundShielded) saves++;
                if (player.IsFortified() && (CustomGameOptions.ShowFortified == FortifyOptions.Self || CustomGameOptions.ShowFortified == FortifyOptions.SelfAndWarden))
                {
                    foreach (var role in Role.GetRoles(RoleEnum.Warden))
                    {
                        var warden = (Warden)role;
                        if (warden.Fortified == player) saves++;
                    }
                }
                if (player.IsShielded() && (CustomGameOptions.ShowShielded == ShieldOptions.Self || CustomGameOptions.ShowShielded == ShieldOptions.SelfAndMedic))
                {
                    foreach (var role in Role.GetRoles(RoleEnum.Medic))
                    {
                        var medic = (Medic)role;
                        if (medic.ShieldedPlayer == player) saves++;
                    }
                }
                if (player.IsBarriered() && (CustomGameOptions.ShowBarriered == BarrierOptions.Self || CustomGameOptions.ShowBarriered == BarrierOptions.SelfAndCleric))
                {
                    foreach (var role in Role.GetRoles(RoleEnum.Cleric))
                    {
                        var cleric = (Cleric)role;
                        if (cleric.Barriered == player) saves++;
                    }
                }
                if ((player.IsProtected() || saviour.Is(RoleEnum.GuardianAngel)) && (CustomGameOptions.ShowProtect == ProtectOptions.Self || CustomGameOptions.ShowProtect == ProtectOptions.SelfAndGA)) saves++;
                if (saves <= 1) ClearVisor(player);
            }
            else if (PlayerControl.LocalPlayer == saviour)
            {
                if (player != FirstRoundShielded) ClearVisor(player);
            }
            else if (PlayerControl.LocalPlayer.Is(RoleEnum.Warden) && player.IsFortified())
            {
                if (CustomGameOptions.ShowFortified == FortifyOptions.Self) ClearVisor(player);
            }
            else if (PlayerControl.LocalPlayer.Is(RoleEnum.Medic) && player.IsShielded())
            {
                if (CustomGameOptions.ShowShielded == ShieldOptions.Self) ClearVisor(player);
            }
            else if (PlayerControl.LocalPlayer.Is(RoleEnum.Cleric) && player.IsBarriered())
            {
                if (CustomGameOptions.ShowBarriered == BarrierOptions.Self) ClearVisor(player);
            }
            else if (PlayerControl.LocalPlayer.Is(RoleEnum.GuardianAngel))
            {
                var ga = Role.GetRole<GuardianAngel>(PlayerControl.LocalPlayer);
                if (ga.target != player || !player.IsProtected()) ClearVisor(player);
                else if (ga.target == player && player.IsProtected() && CustomGameOptions.ShowProtect == ProtectOptions.Self) ClearVisor(player);
            }
            else if (saviour == null) ClearVisor(player);
        }

        public static void ClearVisor(PlayerControl player)
        {
            player.myRend().material.SetColor("_VisorColor", Palette.VisorColor);
            player.myRend().material.SetFloat("_Outline", 0f);
        }

        public static bool IsConcealed(PlayerControl player)
        {
            var role = Role.GetRole(player);

            if (role is Glitch glitch && glitch.IsUsingMimic)
            {
                return true;
            }
            else if (role is Venerer venerer && venerer.IsCamouflaged)
            {
                return true;
            }
            else if (role is Morphling morph && morph.Morphed)
            {
                return true;
            }
            else if (CamouflageUnCamouflage.IsCamoed)
            {
                return true;
            }
            else
            {
                var mushroom = Object.FindObjectOfType<MushroomMixupSabotageSystem>();
                if (mushroom && mushroom.IsActive) return true;
            }

            return false;
        }
    }
}