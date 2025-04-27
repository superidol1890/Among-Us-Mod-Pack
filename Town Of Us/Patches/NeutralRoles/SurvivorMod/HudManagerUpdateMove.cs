using HarmonyLib;
using System;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.SurvivorMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdateMove
    {
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!CustomGameOptions.SurvivorScatter) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Survivor)) return;

            var role = Role.GetRole<Survivor>(PlayerControl.LocalPlayer);

            if (role.TimerText == null)
            {
                if (__instance.UseButton != null) role.TimerText = UnityEngine.Object.Instantiate(__instance.UseButton.cooldownTimerText, __instance.ReportButton.transform);
                else role.TimerText = UnityEngine.Object.Instantiate(__instance.PetButton.cooldownTimerText, __instance.ReportButton.transform);
                role.TimerText.gameObject.SetActive(false);
                role.TimerText.transform.localPosition = new Vector3(
                    role.TimerText.transform.localPosition.x + 0.26f,
                    role.TimerText.transform.localPosition.y + 0.29f,
                    role.TimerText.transform.localPosition.z);
                role.TimerText.transform.localScale = role.TimerText.transform.localScale * 0.65f;
                role.TimerText.alignment = TMPro.TextAlignmentOptions.Right;
                role.TimerText.fontStyle = TMPro.FontStyles.Bold;
            }
            if (role.TimerText != null)
            {
                role.TimerText.text = Math.Ceiling(role.ScatterTimer()) + "";
                if (role.ScatterTimer() >= 10f) role.TimerText.text = "<color=#00FF00FF>" + role.TimerText.text + "</color>";
                else if (role.ScatterTimer() >= 5f) role.TimerText.text = "<color=#FF9900FF>" + role.TimerText.text + "</color>";
                else role.TimerText.text = "<color=#FF0000FF>" + role.TimerText.text + "</color>";
            }
            
            role.TimerText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            if (role.ScatterTimer() <= 0) Utils.RpcMurderPlayer(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer);

            foreach (var location in role.Locations)
            {
                var magnitude = (location - PlayerControl.LocalPlayer.transform.localPosition).magnitude;
                if (magnitude < 5f) return;
            }

            role.LastMoved = DateTime.UtcNow;
            role.Locations.Insert(0, PlayerControl.LocalPlayer.transform.localPosition);
            if (role.Locations.Count > 3)
            {
                role.Locations.RemoveAt(3);
            }
        }
    }
}