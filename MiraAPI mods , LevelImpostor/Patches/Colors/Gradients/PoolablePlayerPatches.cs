using System.Linq;
using HarmonyLib;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Utilities;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(PoolablePlayer))]
public static class PoolablePlayerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromPlayerData))]
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromEitherPlayerDataOrCache))]
    public static void UpdateFromPlayerPrefix(PoolablePlayer __instance, NetworkedPlayerInfo pData)
    {
        if (!pData)
        {
            return;
        }

        __instance.gameObject.SetGradientData(pData.PlayerId);
        var mat = __instance.cosmetics.currentBodySprite.BodySprite.material;
        
        mat.SetFloat(ShaderID.Get("_GradientBlend"), 25f);
        mat.SetFloat(ShaderID.Get("_GradientOffset"), .4f);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromPlayerOutfit))]
    public static void UpdateFromPlayerOutfitPrefix(PoolablePlayer __instance, NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        if (__instance.GetComponent<PlayerGradientData>())
        {
            return;
        }

        var player = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(x => outfit.PlayerName == x.PlayerName);
        if (player == null)
        {
            return;
        }

        __instance.gameObject.SetGradientData(player.PlayerId);

        var mat = __instance.cosmetics.currentBodySprite.BodySprite.material;
        mat.SetFloat(ShaderID.Get("_GradientBlend"), 25f);
        mat.SetFloat(ShaderID.Get("_GradientOffset"), .4f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromPlayerOutfit))]
    public static void UpdateFromPlayerOutfitPostfix(PoolablePlayer __instance, NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        if (__instance.GetComponent<PlayerGradientData>())
        {
            return;
        }

        var mat = __instance.cosmetics.currentBodySprite.BodySprite.material;

        if (AmongUsClientEndGamePatch.PlayerGradientCache.TryGetValue(outfit.PlayerName, out var color))
        {
            mat.SetColor(ShaderID.Get("_BodyColor2"), Palette.PlayerColors[color]);
            mat.SetColor(ShaderID.Get("_BackColor2"), Palette.ShadowColors[color]);
            AmongUsClientEndGamePatch.PlayerGradientCache.Remove(outfit.PlayerName);
        }
    }
}