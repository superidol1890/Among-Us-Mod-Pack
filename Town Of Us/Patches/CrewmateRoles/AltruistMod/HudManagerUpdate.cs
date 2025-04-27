using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.AltruistMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Altruist)) return;
            var reviveButton = __instance.KillButton;

            var role = Role.GetRole<Altruist>(PlayerControl.LocalPlayer);

            if (role.UsesText == null && role.UsesLeft > 0)
            {
                role.UsesText = Object.Instantiate(reviveButton.cooldownTimerText, reviveButton.transform);
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

            reviveButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            role.UsesText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            if (role.CurrentlyReviving) reviveButton.SetCoolDown(role.TimeRemaining, CustomGameOptions.ReviveDuration);
            else reviveButton.SetCoolDown(0f, 1f);

            DeadBody closestBody = GetClosestBody(PlayerControl.LocalPlayer);
            bool canRevive = closestBody == null ? false : Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), closestBody.transform.localPosition)
                <= CustomGameOptions.ReviveRadius * ShipStatus.Instance.MaxLightRadius;

            var renderer = reviveButton.graphic;
            if (role.CurrentlyReviving || (!reviveButton.isCoolingDown && role.ButtonUsable && PlayerControl.LocalPlayer.moveable && canRevive))
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

        public static DeadBody GetClosestBody(PlayerControl refPlayer)
        {
            if (!refPlayer.moveable) return null;
            var num = double.MaxValue;
            var refPosition = refPlayer.GetTruePosition();
            DeadBody result = null;
            foreach (var deadBody in GameObject.FindObjectsOfType<DeadBody>())
            {
                Vector2 playerPosition = deadBody.transform.localPosition;
                var distBetweenPlayers = Vector2.Distance(refPosition, playerPosition);
                var isClosest = distBetweenPlayers < num;
                if (!isClosest) continue;
                num = distBetweenPlayers;
                result = deadBody;
            }

            return result;
        }
    }
}