using HarmonyLib;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.ImpostorRoles.GrenadierMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    public class FlashUnFlash
    {
        static readonly Color normalVision = new Color(0.6f, 0.6f, 0.6f, 0f);
        static readonly Color dimVision = new Color(0.6f, 0.6f, 0.6f, 0.2f);
        static readonly Color blindVision = new Color(0.6f, 0.6f, 0.6f, 1f);

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(HudManager __instance)
        {
            var flashPercent = 0f;
            foreach (var role in Role.GetRoles(RoleEnum.Grenadier))
            {
                var grenadier = (Grenadier) role;
                if (grenadier.Flashed)
                    grenadier.Flash();
                else if (grenadier.Enabled) grenadier.UnFlash();
                if (flashPercent < grenadier.flashPercent) flashPercent = grenadier.flashPercent;
            }
            if (flashPercent > 0f)
            {
                if (ShouldPlayerBeBlinded(PlayerControl.LocalPlayer))
                {
                    ((Renderer)HudManager.Instance.FullScreen).enabled = true;
                    ((Renderer)HudManager.Instance.FullScreen).gameObject.active = true;
                    HudManager.Instance.FullScreen.color = Color.Lerp(normalVision, blindVision, flashPercent);
                }
                else if (ShouldPlayerBeDimmed(PlayerControl.LocalPlayer))
                {
                    ((Renderer)HudManager.Instance.FullScreen).enabled = true;
                    ((Renderer)HudManager.Instance.FullScreen).gameObject.active = true;
                    HudManager.Instance.FullScreen.color = Color.Lerp(normalVision, dimVision, flashPercent);
                }
            }
        }

        public static bool ShouldPlayerBeDimmed(PlayerControl player)
        {
            return (player.Data.IsImpostor() || player.Data.IsDead) && !MeetingHud.Instance;
        }

        public static bool ShouldPlayerBeBlinded(PlayerControl player)
        {
            return !player.Data.IsImpostor() && !player.Data.IsDead && !MeetingHud.Instance;
        }
    }
}