using HarmonyLib;
using Reactor.Utilities;
using TownOfUs.Roles;
using UnityEngine;
using System.Collections;

namespace TownOfUs.CrewmateRoles.JailorMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class MeetingStart
    {
        public static Sprite Jail = TownOfUs.JailCellSprite;

        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Data.IsDead) return;
            if (PlayerControl.LocalPlayer.IsJailed())
            {
                if (PlayerControl.LocalPlayer.Is(Faction.Crewmates))
                {
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "You are jailed, provide relevant information to the Jailor to prove you are Crew");
                }
                else
                {
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "You are jailed, convince the Jailor that you are Crew to avoid being executed");
                }
                if (!PlayerControl.LocalPlayer.IsBlackmailed()) Coroutines.Start(ShowJailed());
            }
            else if (PlayerControl.LocalPlayer.Is(RoleEnum.Jailor))
            {
                var jailor = Role.GetRole<Jailor>(PlayerControl.LocalPlayer);
                if (jailor.Jailed == null || jailor.Jailed.Data.IsDead || jailor.Jailed.Data.Disconnected) return;
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Use /jail to communicate with your jailee");
            }
        }

        public static IEnumerator ShowJailed()
        {
            yield return HudManager.Instance.CoFadeFullScreen(Color.clear, new Color(0f, 0f, 0f, 0.8f));
            var TempPosition = HudManager.Instance.shhhEmblem.transform.localPosition;
            var TempDuration = HudManager.Instance.shhhEmblem.HoldDuration;
            HudManager.Instance.shhhEmblem.transform.localPosition = new Vector3(
                HudManager.Instance.shhhEmblem.transform.localPosition.x,
                HudManager.Instance.shhhEmblem.transform.localPosition.y,
                HudManager.Instance.FullScreen.transform.position.z + 1f);
            HudManager.Instance.shhhEmblem.TextImage.text = "YOU ARE JAILED";
            HudManager.Instance.shhhEmblem.Body.sprite = Jail;
            HudManager.Instance.shhhEmblem.Hand.sprite = null;
            HudManager.Instance.shhhEmblem.Background.sprite = null;
            HudManager.Instance.shhhEmblem.HoldDuration = 2.5f;
            yield return HudManager.Instance.ShowEmblem(true);
            HudManager.Instance.shhhEmblem.transform.localPosition = TempPosition;
            HudManager.Instance.shhhEmblem.HoldDuration = TempDuration;
            yield return HudManager.Instance.CoFadeFullScreen(new Color(0f, 0f, 0f, 0.8f), Color.clear);
            yield return null;
        }
    }
}
