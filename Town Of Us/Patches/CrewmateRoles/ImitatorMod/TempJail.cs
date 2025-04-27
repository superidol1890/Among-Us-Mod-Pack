using Reactor.Utilities.Extensions;
using System.Collections.Generic;
using TownOfUs.Roles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.CrewmateRoles.ImitatorMod
{
    public class TempJail
    {
        public static Sprite CellSprite => TownOfUs.InJailSprite;

        public static void GenCell(Imitator role, PlayerVoteArea voteArea)
        {
            role.JailCell.Destroy();
            var confirmButton = voteArea.Buttons.transform.GetChild(0).gameObject;
            var parent = confirmButton.transform.parent.parent;

            var jailCell = Object.Instantiate(confirmButton, voteArea.transform);
            var cellRenderer = jailCell.GetComponent<SpriteRenderer>();
            var passive = jailCell.GetComponent<PassiveButton>();
            cellRenderer.sprite = CellSprite;
            jailCell.transform.localPosition = new Vector3(-0.95f, 0f, -2f);
            jailCell.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            jailCell.layer = 5;
            jailCell.transform.parent = parent;
            jailCell.transform.GetChild(0).gameObject.Destroy();

            passive.OnClick = new Button.ButtonClickedEvent();
            role.JailCell = jailCell;
        }

        public static void AddTempJail(MeetingHud __instance)
        {
            List<PlayerControl> jailed = new List<PlayerControl>();
            foreach (var role in Role.GetRoles(RoleEnum.Imitator))
            {
                var imitator = (Imitator)role;
                if (imitator.jailedPlayer == null) return;
                if (imitator.Player.Data.IsDead || imitator.Player.Data.Disconnected) return;
                if (imitator.jailedPlayer.Data.IsDead || imitator.jailedPlayer.Data.Disconnected) return;
                foreach (var voteArea in __instance.playerStates)
                {
                    if (imitator.jailedPlayer.PlayerId == voteArea.TargetPlayerId)
                    {
                        if (!jailed.Contains(imitator.jailedPlayer)) GenCell(imitator, voteArea);
                        else
                        {
                            imitator.jailedPlayer = null;
                            break;
                        }
                    }
                }
                jailed.Add(imitator.jailedPlayer);
            }
        }
    }
}