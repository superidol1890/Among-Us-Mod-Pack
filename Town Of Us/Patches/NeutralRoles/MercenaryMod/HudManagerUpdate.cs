using HarmonyLib;
using System.Linq;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.MercenaryMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate
    {
        public static Sprite GuardSprite => TownOfUs.GuardSprite;
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Mercenary)) return;
            var bribeButton = __instance.KillButton;

            var role = Role.GetRole<Mercenary>(PlayerControl.LocalPlayer);

            if (role.GuardButton == null)
            {
                role.GuardButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.GuardButton.graphic.enabled = true;
                role.GuardButton.gameObject.SetActive(false);
            }

            if (role.UsesText == null && role.UsesLeft > 0)
            {
                role.UsesText = Object.Instantiate(role.GuardButton.cooldownTimerText, role.GuardButton.transform);
                role.UsesText.gameObject.SetActive(false);
                role.UsesText.transform.localPosition = new Vector3(
                    role.UsesText.transform.localPosition.x + 0.26f,
                    role.UsesText.transform.localPosition.y + 0.29f,
                    role.UsesText.transform.localPosition.z);
                role.UsesText.transform.localScale = role.UsesText.transform.localScale * 0.65f;
                role.UsesText.alignment = TMPro.TextAlignmentOptions.Right;
                role.UsesText.fontStyle = TMPro.FontStyles.Bold;
            }
            if (role.UsesText != null)
            {
                role.UsesText.text = role.UsesLeft + "";
            }

            if (role.GoldText == null)
            {
                role.GoldText = Object.Instantiate(__instance.KillButton.cooldownTimerText, __instance.KillButton.transform);
                role.GoldText.gameObject.SetActive(false);
                role.GoldText.transform.localPosition = new Vector3(
                    role.GoldText.transform.localPosition.x + 0.26f,
                    role.GoldText.transform.localPosition.y + 0.29f,
                    role.GoldText.transform.localPosition.z);
                role.GoldText.transform.localScale = role.GoldText.transform.localScale * 0.65f;
                role.GoldText.alignment = TMPro.TextAlignmentOptions.Right;
                role.GoldText.fontStyle = TMPro.FontStyles.Bold;
                role.GoldText.enableWordWrapping = false;
            }
            if (role.GoldText != null)
            {
                role.GoldText.text = role.Gold + "/" + CustomGameOptions.GoldToBribe + "";
                if (role.Gold >= CustomGameOptions.GoldToBribe) role.GoldText.text = "<color=#00FF00FF>" + role.GoldText.text + "</color>";
                else role.GoldText.text = "<color=#FFFFFFFF>" + role.GoldText.text + "</color>";
            }

            role.GuardButton.graphic.sprite = GuardSprite;
            role.GuardButton.transform.localPosition = new Vector3(-2f, 0f, 0f);
            
            __instance.KillButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            role.GuardButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            role.GoldText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            role.UsesText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            __instance.KillButton.SetCoolDown(0f, 1f);
            role.GuardButton.SetCoolDown(role.GuardTimer(), CustomGameOptions.GuardCd);

            var notGuarded = PlayerControl.AllPlayerControls.ToArray().Where(
                player => !role.Guarded.Contains(player.PlayerId)).ToList();
            var notBribed = PlayerControl.AllPlayerControls.ToArray().Where(
                player => !role.Bribed.Contains(player.PlayerId)).ToList();

            if (role.Gold < CustomGameOptions.GoldToBribe)
            {
                __instance.KillButton.SetTarget(null);
                role.ClosestBribePlayer = null;
            }
            else Utils.SetTarget(ref role.ClosestBribePlayer, __instance.KillButton, float.NaN, notBribed);
            if (role.ButtonUsable) Utils.SetTarget(ref role.ClosestGuardPlayer, role.GuardButton, float.NaN, notGuarded);
            else role.GuardButton.SetTarget(null);

            var renderer = role.GuardButton.graphic;
            var renderer2 = __instance.KillButton.graphic;

            if (role.ClosestBribePlayer != null && PlayerControl.LocalPlayer.moveable)
            {
                renderer2.color = Palette.EnabledColor;
                renderer2.material.SetFloat("_Desat", 0f);
                role.GoldText.color = Palette.EnabledColor;
                role.GoldText.material.SetFloat("_Desat", 0f);
            }
            else
            {
                renderer2.color = Palette.DisabledClear;
                renderer2.material.SetFloat("_Desat", 1f);
                role.GoldText.color = Palette.DisabledClear;
                role.GoldText.material.SetFloat("_Desat", 1f);
            }

            if (role.ClosestGuardPlayer != null && role.ButtonUsable)
            {
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
                role.UsesText.color = Palette.EnabledColor;
                role.UsesText.material.SetFloat("_Desat", 0f);
            }
            else
            {
                renderer.color = Palette.DisabledClear;
                renderer.material.SetFloat("_Desat", 1f);
                role.UsesText.color = Palette.DisabledClear;
                role.UsesText.material.SetFloat("_Desat", 1f);
            }
        }
    }
}