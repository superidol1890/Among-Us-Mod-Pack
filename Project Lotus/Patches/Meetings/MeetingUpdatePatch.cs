using HarmonyLib;
using Lotus.API;
using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;

namespace Lotus.Patches.Meetings;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
class MeetingUpdatePatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(MeetingUpdatePatch));

    public static void Postfix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.LeftControl))
            __instance.playerStates.DoIf(x => x.HighlightedFX.enabled, x =>
            {
                PlayerControl? player = Utils.GetPlayerById(x.TargetPlayerId);
                ProtectedRpc.CheckMurder(PlayerControl.LocalPlayer, player);

                log.High($"Execute: {player.GetNameWithRole()}", "Execution");
            });
    }
}