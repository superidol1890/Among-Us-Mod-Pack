using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;
using System.Linq;

namespace TownOfUs.CrewmateRoles.OracleMod
{
    [HarmonyPatch(typeof(HudManager))]
    public class HudConfess
    {
        public static Sprite Bless => TownOfUs.BlessSprite;

        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Oracle)) return;
            var confessButton = __instance.KillButton;

            var role = Role.GetRole<Oracle>(PlayerControl.LocalPlayer);

            if (role.BlessButton == null)
            {
                role.BlessButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.BlessButton.graphic.enabled = true;
                role.BlessButton.gameObject.SetActive(false);
            }

            if (PlayerControl.LocalPlayer.Data.IsDead) role.BlessButton.SetTarget(null);

            var notBlessed = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(x => x != role.Blessed)
                .ToList();

            role.BlessButton.graphic.sprite = Bless;
            role.BlessButton.transform.localPosition = new Vector3(-2f, 0f, 0f);

            role.BlessButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            role.BlessButton.SetCoolDown(role.BlessTimer(), CustomGameOptions.BlessCd);
            if (PlayerControl.LocalPlayer.moveable) Utils.SetTarget(ref role.ClosestBlessedPlayer, role.BlessButton, float.NaN, notBlessed);
            else role.BlessButton.SetTarget(null);

            confessButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            confessButton.SetCoolDown(role.ConfessTimer(), CustomGameOptions.ConfessCd);

            var notConfessing = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(x => x != role.Confessor)
                .ToList();

            Utils.SetTarget(ref role.ClosestPlayer, confessButton, float.NaN, notConfessing);
        }
    }
}
