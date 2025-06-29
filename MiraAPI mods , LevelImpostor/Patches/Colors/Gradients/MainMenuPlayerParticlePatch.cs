using HarmonyLib;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Features;
using UnityEngine;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(PlayerParticles),nameof(PlayerParticles.PlacePlayer))]
public static class MainMenuPlayerParticlePatch
{
    public static void Postfix(PlayerParticle part)
    {
        part.myRend.material = LaunchpadAssets.GradientMaterial.LoadAsset();
        var id = part.gameObject.AddComponent<PlayerGradientData>().GradientColor;
        part.GetComponent<GradientColorComponent>().SetColor(Random.RandomRangeInt(0,Palette.PlayerColors.Length), id);
    }
}