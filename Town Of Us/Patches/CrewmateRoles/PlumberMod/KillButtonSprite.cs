using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.PlumberMod
{
    [HarmonyPatch(typeof(HudManager))]
    public class VentButtonSprite
    {
        public static Sprite Flush = TownOfUs.FlushSprite;
        public static Sprite Vent = new Sprite();

        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Plumber)) return;
            var isDead = PlayerControl.LocalPlayer.Data.IsDead;
            var flushButton = __instance.ImpostorVentButton;
            var blockButton = __instance.KillButton;
            
            var role = Role.GetRole<Plumber>(PlayerControl.LocalPlayer);

            if (role.UsesText == null && role.UsesLeft > 0)
            {
                role.UsesText = Object.Instantiate(blockButton.cooldownTimerText, blockButton.transform);
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

            flushButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            blockButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            role.UsesText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            if (flushButton.isActiveAndEnabled)
            {
                flushButton.transform.localPosition = new Vector3(-2f, 0f, 0f);
                if (flushButton.graphic.sprite != Flush)
                {
                    flushButton.cooldownTimerText = Object.Instantiate(__instance.KillButton.cooldownTimerText, flushButton.transform);
                    Vent = flushButton.graphic.sprite;
                }
                flushButton.graphic.sprite = Flush;
                flushButton.buttonLabelText.text = "";
                flushButton.SetCoolDown(role.FlushTimer(), CustomGameOptions.FlushCd);
                role.Vent = flushButton.currentTarget;
            }

            blockButton.SetTarget(null);
            blockButton.SetCoolDown(role.FlushTimer(), CustomGameOptions.FlushCd);
            var flushRenderer = flushButton.graphic;
            var renderer = blockButton.graphic;

            if (role.Vent != null && role.ButtonUsable & blockButton.enabled && PlayerControl.LocalPlayer.moveable)
            {
                flushRenderer.color = Palette.EnabledColor;
                flushRenderer.material.SetFloat("_Desat", 0f);
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
                role.UsesText.color = Palette.EnabledColor;
                role.UsesText.material.SetFloat("_Desat", 0f);
            }
            else
            {
                flushRenderer.color = Palette.DisabledClear;
                flushRenderer.material.SetFloat("_Desat", 1f);
                renderer.color = Palette.DisabledClear;
                renderer.material.SetFloat("_Desat", 1f);
                role.UsesText.color = Palette.DisabledClear;
                role.UsesText.material.SetFloat("_Desat", 1f);
            }
        }
    }
}