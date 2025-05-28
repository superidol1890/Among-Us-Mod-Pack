using HarmonyLib;
using Lotus.API;
using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;
using System.Linq;

namespace Lotus.Patches.Meetings;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
class MeetingDisconnectPatch
{
    public static void Postfix(MeetingHud __instance, PlayerControl pc, DisconnectReasons reason)
    {
        if (AmongUsClient.Instance.AmHost) return; // return if we ARE host.

        PlayerVoteArea playerVoteArea = __instance.playerStates.First(pv => pv.TargetPlayerId == pc.PlayerId);
        playerVoteArea.AmDead = true;
        playerVoteArea.Overlay.gameObject.SetActive(true);
    }
}