using HarmonyLib;
using System;
using UnityEngine;
using TownOfUs.Extensions;
using AmongUs.GameOptions;

namespace TownOfUs
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    public static class PatchKillTimer
    {
        public static bool GameStarted = false;
        [HarmonyPriority(Priority.First)]
        public static void Prefix(PlayerControl __instance, ref float time)
        {
            if (__instance.Data.IsImpostor() && time <= 11f
                && Math.Abs(__instance.killTimer - time) > 2 * Time.deltaTime
                && GameStarted == false)
            {
                if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek)
                    time = GameOptionsManager.Instance.currentHideNSeekGameOptions.KillCooldown - 0.25f;
                else time = CustomGameOptions.InitialCooldowns - 0.25f;
                GameStarted = true;
            }
        }
    }
}