using HarmonyLib;
using Reactor.Utilities;
using TownOfUs.CrewmateRoles.MedicMod;
using static TownOfUs.Roles.Glitch;

namespace TownOfUs.NeutralRoles.GlitchMod
{
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    public class StopReport
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ReportButton __instance)
        {
            if (PlayerControl.LocalPlayer.IsHacked())
            {
                Coroutines.Start(AbilityCoroutine.Hack(PlayerControl.LocalPlayer));
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    public class StopClickReport
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(DeadBody __instance)
        {
            if (!HudManager.Instance.ReportButton.isActiveAndEnabled) return false;
            foreach (var death in Murder.KilledPlayers)
            {
                if (death.KillerId == PlayerControl.LocalPlayer.PlayerId && death.PlayerId == __instance.ParentId &&
                    PlayerControl.LocalPlayer.Is(RoleEnum.SoulCollector)) return false;
                if (death.KillerId == PlayerControl.LocalPlayer.PlayerId && death.PlayerId == __instance.ParentId && 
                    ((PlayerControl.LocalPlayer.Is(RoleEnum.Sheriff) && !CustomGameOptions.SheriffBodyReport) ||
                    (PlayerControl.LocalPlayer.Is(RoleEnum.Hunter) && !CustomGameOptions.HunterBodyReport)))
                    return false;
            }
            if (PlayerControl.LocalPlayer.IsHacked())
            {
                Coroutines.Start(AbilityCoroutine.Hack(PlayerControl.LocalPlayer));
                return false;
            }
            return true;
        }
    }
}
