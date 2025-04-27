using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.ImpostorRoles.EclipsalMod
{
    public class Blind
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class HudManagerUpdate
        {
            public static void Postfix(HudManager __instance)
            {
                foreach (var role in Role.GetRoles(RoleEnum.Eclipsal))
                {
                    var eclipsal = (Eclipsal)role;
                    if (eclipsal.BlindPlayers.Contains(PlayerControl.LocalPlayer))
                    {
                        try
                        {
                            HudManager.Instance.ReportButton.SetActive(false);
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
    }
}