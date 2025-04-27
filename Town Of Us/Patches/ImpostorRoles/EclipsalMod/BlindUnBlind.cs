using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.ImpostorRoles.EclipsalMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    public class BlindUnBlind
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(HudManager __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Eclipsal))
            {
                var eclipsal = (Eclipsal)role;
                if (eclipsal.Blinded)
                    eclipsal.Blind();
                else if (eclipsal.Enabled) eclipsal.UnBlind();
            }
        }
    }
}