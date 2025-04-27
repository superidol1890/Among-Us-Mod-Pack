using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.ClericMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    public class ProtectUnportect
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(HudManager __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Cleric))
            {
                var cler = (Cleric) role;
                if (cler.Barriered != null)
                {
                    cler.TimeRemaining -= Time.deltaTime;
                    if (cler.TimeRemaining <= 0f) cler.UnBarrier();
                }
            }
        }
    }
}