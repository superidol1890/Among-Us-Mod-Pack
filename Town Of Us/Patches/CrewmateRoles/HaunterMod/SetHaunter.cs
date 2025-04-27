using System;
using System.Collections;
using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using TownOfUs.Patches;
using System.Linq;
using System.Collections.Generic;

namespace TownOfUs.CrewmateRoles.HaunterMod
{
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public static class AirshipExileController_WrapUpAndSpawn
    {
        public static void Postfix(AirshipExileController __instance) => SetHaunter.ExileControllerPostfix(__instance);
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class SetHaunter
    {
        public static PlayerControl WillBeHaunter;
        public static bool HaunterOn;
        public static Vector2 StartPosition;

        public static void ExileControllerPostfix(ExileController __instance)
        {
            if (!HaunterOn || !AmongUsClient.Instance.AmHost) return;

            if (WillBeHaunter == null)
            {
                var toChooseFrom = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Crewmates) && !x.IsLover() && !x.Data.Disconnected && x.Data.IsDead).ToList();
                if (toChooseFrom.Count == 0) return;
                var rand = Random.RandomRangeInt(0, toChooseFrom.Count);
                var pc = toChooseFrom[rand];
                WillBeHaunter = pc;
                Utils.Rpc(CustomRPC.SetHaunter, pc.PlayerId);
                ChangeToHaunter();
            }

            if (Role.GetRole<Haunter>(WillBeHaunter).Caught) return;

            List<Vent> vents = new();
            var CleanVentTasks = WillBeHaunter.myTasks.ToArray().Where(x => x.TaskType == TaskTypes.VentCleaning).ToList();
            if (CleanVentTasks != null)
            {
                var ids = CleanVentTasks.Where(x => !x.IsComplete)
                                        .ToList()
                                        .ConvertAll(x => x.FindConsoles()[0].ConsoleId);

                vents = ShipStatus.Instance.AllVents.Where(x => !ids.Contains(x.Id)).ToList();
            }
            else vents = ShipStatus.Instance.AllVents.ToList();

            var startingVent = vents[Random.RandomRangeInt(0, vents.Count)];

            Utils.Rpc(CustomRPC.SetPos, WillBeHaunter.PlayerId, startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f);
            var pos = new Vector2(startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f);
            WillBeHaunter.transform.position = pos;
            WillBeHaunter.NetTransform.SnapTo(pos);
        }

        public static void ChangeToHaunter()
        {
            if (WillBeHaunter.Is(RoleEnum.Plumber))
            {
                var plumberRole = Role.GetRole<Plumber>(WillBeHaunter);
                foreach (GameObject barricade in plumberRole.Barricades)
                {
                    UnityEngine.Object.Destroy(barricade);
                }
            }

            if (WillBeHaunter.Is(RoleEnum.Cleric))
            {
                var clericRole = Role.GetRole<Cleric>(WillBeHaunter);
                if (clericRole.Barriered != null) clericRole.UnBarrier();
            }

            var oldRole = Role.GetRole(WillBeHaunter);
            var killsList = (oldRole.CorrectKills, oldRole.IncorrectKills, oldRole.CorrectAssassinKills, oldRole.IncorrectAssassinKills);
            Role.RoleDictionary.Remove(WillBeHaunter.PlayerId);
            if (PlayerControl.LocalPlayer == WillBeHaunter)
            {
                if (SubmergedCompatibility.Loaded && GameOptionsManager.Instance.currentNormalGameOptions.MapId == 6)
                {
                    HudUpdate.Zooming = false;
                    HudUpdate.ZoomStart();
                }
                var role = new Haunter(PlayerControl.LocalPlayer);
                role.formerRole = oldRole.RoleType;
                role.CorrectKills = killsList.CorrectKills;
                role.IncorrectKills = killsList.IncorrectKills;
                role.CorrectAssassinKills = killsList.CorrectAssassinKills;
                role.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                role.RegenTask();
            }
            else
            {
                var role = new Haunter(WillBeHaunter);
                role.formerRole = oldRole.RoleType;
                role.CorrectKills = killsList.CorrectKills;
                role.IncorrectKills = killsList.IncorrectKills;
                role.CorrectAssassinKills = killsList.CorrectAssassinKills;
                role.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
            }

            Utils.RemoveTasks(WillBeHaunter);
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Phantom)) WillBeHaunter.MyPhysics.ResetMoveState();

            WillBeHaunter.gameObject.layer = LayerMask.NameToLayer("Players");

            WillBeHaunter.gameObject.GetComponent<PassiveButton>().OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            WillBeHaunter.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => WillBeHaunter.OnClick()));
            WillBeHaunter.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }

        public static IEnumerator WaitForMeeting()
        {
            var startTime = DateTime.UtcNow;
            while (true)
            {
                if ((DateTime.UtcNow - startTime).TotalSeconds > 0.2f) break;
                yield return null;
            }
            if (PlayerControl.LocalPlayer == WillBeHaunter) Role.GetRole(PlayerControl.LocalPlayer).RegenTask();
            WillBeHaunter.gameObject.layer = LayerMask.NameToLayer("Players");
        }

        public static void Postfix(ExileController __instance) => ExileControllerPostfix(__instance);

        [HarmonyPatch(typeof(Object), nameof(Object.Destroy), new Type[] { typeof(GameObject) })]
        public static void Prefix(GameObject obj)
        {
            if (!SubmergedCompatibility.Loaded || GameOptionsManager.Instance?.currentNormalGameOptions?.MapId != 6) return;
            if (obj.name?.Contains("ExileCutscene") == true) ExileControllerPostfix(ExileControllerPatch.lastExiled);
        }
    }
}