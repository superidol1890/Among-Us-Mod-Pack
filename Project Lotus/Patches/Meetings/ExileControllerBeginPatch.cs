using Lotus.API;
using HarmonyLib;
using Lotus.Roles;
using System.Linq;
using Lotus.API.Player;
using Lotus.Extensions;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using static Lotus.API.Vanilla.Meetings.MeetingDelegate;
using Lotus.API.Odyssey;

namespace Lotus.Patches.Meetings;
[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public class ExileControllerBeginPatch
{
    public static void Postfix(ExileController __instance, ExileController.InitProperties init)
    {
        if (!AUSettings.ConfirmImpostor() || !AmongUsClient.Instance.AmHost || init.networkedPlayer == null) return;
        CustomRole roleDefinition;
        string name;

        PlayerControl? player = Players.GetAllPlayers().FirstOrDefault(p => p.PlayerId == init.networkedPlayer.PlayerId);
        if (player != null)
        {
            roleDefinition = player.PrimaryRole();
            name = player.name;
        }
        else
        {
            roleDefinition = Game.MatchData.Roles.GetMainRole(init.networkedPlayer.PlayerId);
            name = init.outfit.PlayerName;
        }

        int impostors = Players.GetPlayers(PlayerFilter.Impostor | PlayerFilter.Alive).Count();

        string textFormatting = "<size=2.5>" + RoleRevealText.Formatted(name, roleDefinition.RoleColor.Colorize(roleDefinition.RoleName));
        textFormatting += "\n" + (impostors == 0 ? NoImpostorsText : RemainingImpostorsText.Formatted(impostors));
        __instance.completeString = textFormatting;
    }
}
