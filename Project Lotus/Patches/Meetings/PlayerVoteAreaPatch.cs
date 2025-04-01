using HarmonyLib;
using Lotus.API.Odyssey;
using UnityEngine;

namespace Lotus.Patches.Meetings;

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Start))]
public class PlayerVoteAreaPatch
{
    public static void Postfix(PlayerVoteArea __instance) // This patch crashes the game for whatever reason. I've done some things to fix it.
    {
        if (Game.State is GameState.InLobby) return;
        if (__instance == null || __instance.ColorBlindName == null) return;
        if (__instance.ColorBlindName.transform == null || __instance.ColorBlindName.transform.parent == null) return;
        try
        {
            __instance.ColorBlindName.transform.localPosition -= new Vector3(1.25f, 0.15f);
        }
        catch { }
    }
}