using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.NeutralRoles.MercenaryMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class MeetingStart
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.Is(Faction.NeutralEvil) ||
                PlayerControl.LocalPlayer.Is(RoleEnum.Amnesiac) || PlayerControl.LocalPlayer.Is(RoleEnum.Mercenary)) return;
            foreach (var role in Role.GetRoles(RoleEnum.Mercenary))
            {
                var merc = (Mercenary)role;
                if (merc.Bribed.Contains(PlayerControl.LocalPlayer.PlayerId) && !merc.Alert)
                {
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "You have been bribed by a Mercenary!");
                    merc.Alert = true;
                }
            }
        }
    }
}
