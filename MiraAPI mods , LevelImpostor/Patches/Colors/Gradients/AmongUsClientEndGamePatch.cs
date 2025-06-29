using System.Collections.Generic;
using HarmonyLib;
using LaunchpadReloaded.Features.Managers;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class AmongUsClientEndGamePatch
{
    public static Dictionary<string, byte> PlayerGradientCache { get; } = [];

    public static void Prefix()
    {
        foreach (var player in GameData.Instance.AllPlayers)
        {
            if (GradientManager.TryGetColor(player.PlayerId, out var color))
            {
                PlayerGradientCache[player.PlayerName] = color;
            }
        }
    }
}