using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.ClericMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdate
    {
        public static Sprite CleanseSprite => TownOfUs.CleanseSprite;
        
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Cleric)) return;
            var role = Role.GetRole<Cleric>(PlayerControl.LocalPlayer);

            if (role.CleanseButton == null)
            {
                role.CleanseButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.CleanseButton.graphic.enabled = true;
                role.CleanseButton.gameObject.SetActive(false);
            }

            role.CleanseButton.graphic.sprite = CleanseSprite;
            role.CleanseButton.transform.localPosition = new Vector3(-2f, 0f, 0f);

            if (PlayerControl.LocalPlayer.Data.IsDead) role.CleanseButton.SetTarget(null);

            __instance.KillButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            role.CleanseButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            __instance.KillButton.SetCoolDown(role.BarrierTimer(), CustomGameOptions.BarrierCd);
            role.CleanseButton.SetCoolDown(role.BarrierTimer(), CustomGameOptions.BarrierCd);

            Utils.SetTarget(ref role.ClosestPlayer, role.CleanseButton, float.NaN);
            if (role.Barriered == null) __instance.KillButton.SetTarget(role.ClosestPlayer);
            else
            {
                __instance.KillButton.SetTarget(null);
                var renderer = __instance.KillButton.graphic;
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
            }

            return;
        }
    }
}
