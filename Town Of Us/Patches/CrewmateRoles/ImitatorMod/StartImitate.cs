using System;
using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;
using Object = UnityEngine.Object;
using TownOfUs.Patches;
using System.Linq;
using TownOfUs.Extensions;
using System.Collections.Generic;

namespace TownOfUs.CrewmateRoles.ImitatorMod
{
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public static class AirshipExileController_WrapUpAndSpawn
    {
        public static void Postfix(AirshipExileController __instance) => StartImitate.ExileControllerPostfix(__instance);
    }
    
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class StartImitate
    {
        public static List<byte> ImitatingPlayers = new List<byte>();
        public static void ExileControllerPostfix(ExileController __instance)
        {
            var exiled = __instance.initData.networkedPlayer?.Object;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Imitator)) return;
            if (PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.Data.Disconnected) return;
            if (exiled == PlayerControl.LocalPlayer) return;

            var imitator = Role.GetRole<Imitator>(PlayerControl.LocalPlayer);
            if (imitator.ImitatePlayer == null || imitator.ImitatePlayer.Is(RoleEnum.Imitator)) return;

            Imitate(imitator);

            Utils.Rpc(CustomRPC.StartImitate, imitator.Player.PlayerId);
        }

        public static void Postfix(ExileController __instance) => ExileControllerPostfix(__instance);

        [HarmonyPatch(typeof(Object), nameof(Object.Destroy), new Type[] { typeof(GameObject) })]
        public static void Prefix(GameObject obj)
        {
            if (!SubmergedCompatibility.Loaded || GameOptionsManager.Instance?.currentNormalGameOptions?.MapId != 6) return;
            if (obj.name?.Contains("ExileCutscene") == true) ExileControllerPostfix(ExileControllerPatch.lastExiled);
        }

        public static Sprite Sprite => TownOfUs.Arrow;

        public static void Imitate(Imitator imitator)
        {
            if (imitator.ImitatePlayer == null) return;
            var imi = imitator.Player;
            ImitatingPlayers.Add(imi.PlayerId);
            var imitatorRole = Role.GetRole(imitator.ImitatePlayer).RoleType;
            if (imitatorRole == RoleEnum.Haunter)
            {
                var haunter = Role.GetRole<Haunter>(imitator.ImitatePlayer);
                imitatorRole = haunter.formerRole;
            }
            var role = Role.GetRole(imi);
            var killsList = (role.Kills, role.CorrectKills, role.IncorrectKills, role.CorrectAssassinKills, role.IncorrectAssassinKills);
            Role.RoleDictionary.Remove(imi.PlayerId);
            if (imitatorRole == RoleEnum.Crewmate) new Crewmate(imi);
            else if (imitatorRole == RoleEnum.Aurial) new Aurial(imi);
            else if (imitatorRole == RoleEnum.Detective) new Detective(imi);
            else if (imitatorRole == RoleEnum.Investigator) new Investigator(imi);
            else if (imitatorRole == RoleEnum.Lookout) new Lookout(imi);
            else if (imitatorRole == RoleEnum.Mystic) new Mystic(imi);
            else if (imitatorRole == RoleEnum.Oracle) new Oracle(imi);
            else if (imitatorRole == RoleEnum.Seer) new Seer(imi);
            else if (imitatorRole == RoleEnum.Snitch)
            {
                var snitch = new Snitch(imi);
                var taskinfos = imi.Data.Tasks.ToArray();
                var tasksLeft = taskinfos.Count(x => !x.Complete);
                if (tasksLeft <= CustomGameOptions.SnitchTasksRemaining && ((PlayerControl.LocalPlayer.Data.IsImpostor() && (Role.GetRole(PlayerControl.LocalPlayer).formerRole == RoleEnum.None || CustomGameOptions.SnitchSeesTraitor))
                            || (PlayerControl.LocalPlayer.Is(Faction.NeutralKilling) && CustomGameOptions.SnitchSeesNeutrals)))
                {
                    var gameObj = new GameObject();
                    var arrow = gameObj.AddComponent<ArrowBehaviour>();
                    gameObj.transform.parent = PlayerControl.LocalPlayer.gameObject.transform;
                    var renderer = gameObj.AddComponent<SpriteRenderer>();
                    renderer.sprite = Sprite;
                    arrow.image = renderer;
                    gameObj.layer = 5;
                    snitch.ImpArrows.Add(arrow);
                }
                else if (tasksLeft == 0 && PlayerControl.LocalPlayer == imi)
                {
                    var impostors = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data.IsImpostor());
                    foreach (var imp in impostors)
                    {
                        if (Role.GetRole(imp).formerRole == RoleEnum.None || CustomGameOptions.SnitchSeesTraitor)
                        {
                            var gameObj = new GameObject();
                            var arrow = gameObj.AddComponent<ArrowBehaviour>();
                            gameObj.transform.parent = PlayerControl.LocalPlayer.gameObject.transform;
                            var renderer = gameObj.AddComponent<SpriteRenderer>();
                            renderer.sprite = Sprite;
                            arrow.image = renderer;
                            gameObj.layer = 5;
                            snitch.SnitchArrows.Add(imp.PlayerId, arrow);
                        }
                    }
                }
            }
            else if (imitatorRole == RoleEnum.Spy) new Spy(imi);
            else if (imitatorRole == RoleEnum.Tracker) new Tracker(imi);
            else if (imitatorRole == RoleEnum.Trapper) new Trapper(imi);
            else if (imitatorRole == RoleEnum.Deputy)
            {
                var deputy = new Deputy(imi);
                deputy.StartingCooldown = deputy.StartingCooldown.AddSeconds(-10f);
            }
            else if (imitatorRole == RoleEnum.Hunter) new Hunter(imi);
            else if (imitatorRole == RoleEnum.Jailor) new Jailor(imi);
            else if (imitatorRole == RoleEnum.Sheriff) new Sheriff(imi);
            else if (imitatorRole == RoleEnum.Veteran) new Veteran(imi);
            else if (imitatorRole == RoleEnum.Vigilante) new Vigilante(imi);
            else if (imitatorRole == RoleEnum.Altruist) new Altruist(imi);
            else if (imitatorRole == RoleEnum.Cleric) new Cleric(imi);
            else if (imitatorRole == RoleEnum.Medic)
            {
                var medic = new Medic(imi);
                medic.StartingCooldown = medic.StartingCooldown.AddSeconds(-10f);
            }
            else if (imitatorRole == RoleEnum.Warden)
            {
                var warden = new Warden(imi);
                warden.StartingCooldown = warden.StartingCooldown.AddSeconds(-10f);
            }
            else if (imitatorRole == RoleEnum.Engineer) new Engineer(imi);
            else if (imitatorRole == RoleEnum.Mayor) new Mayor(imi);
            else if (imitatorRole == RoleEnum.Medium) new Medium(imi);
            else if (imitatorRole == RoleEnum.Plumber) new Plumber(imi);
            else if (imitatorRole == RoleEnum.Politician) new Politician(imi);
            else if (imitatorRole == RoleEnum.Prosecutor) new Prosecutor(imi);
            else if (imitatorRole == RoleEnum.Swapper) new Swapper(imi);
            else if (imitatorRole == RoleEnum.Transporter) new Transporter(imi);

            var newRole = Role.GetRole(imi);
            newRole.RemoveFromRoleHistory(newRole.RoleType);
            newRole.Kills = killsList.Kills;
            newRole.CorrectKills = killsList.CorrectKills;
            newRole.IncorrectKills = killsList.IncorrectKills;
            newRole.CorrectAssassinKills = killsList.CorrectAssassinKills;
            newRole.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
        }
    }
}