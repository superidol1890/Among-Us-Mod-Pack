using HarmonyLib;
using Hazel;
using Lotus.Extensions;

namespace Lotus.RPC;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
[HarmonyPriority(Priority.VeryHigh)]
public class RpcCheckForOtherMods
{
    internal const byte TOHVersionCheck = 80; // mostly every mod has this set to 80. EHR is just the odd one out.
    internal const byte EHRVersionCheck = 78;
    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (callId is TOHVersionCheck)
            AmongUsClient.Instance.KickPlayerWithMessage(__instance, $"{__instance.name} was kicked for joining with a variation of Town Of Host.");
        else if (callId is EHRVersionCheck)
            AmongUsClient.Instance.KickPlayerWithMessage(__instance, $"{__instance.name} was kicked for joining with Endless Host Roles.");
    }
}