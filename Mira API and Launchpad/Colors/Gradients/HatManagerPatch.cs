using HarmonyLib;
using LaunchpadReloaded.Features;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(HatManager),nameof(HatManager.Initialize))]
public static class HatManagerPatch
{
    public static void Postfix(HatManager __instance)
    {
        var mat1 = __instance.PlayerMaterial = LaunchpadAssets.GradientMaterial.LoadAsset();
        var mat2 = __instance.MaskedPlayerMaterial = LaunchpadAssets.MaskedGradientMaterial.LoadAsset();
        
        mat1.SetFloat(ShaderID.Get("_GradientBlend"), 1);
        mat2.SetFloat(ShaderID.Get("_GradientBlend"), 1);

        mat1.SetFloat(ShaderID.Get("_GradientAngle"), 225);
        mat2.SetFloat(ShaderID.Get("_GradientAngle"), 225);
        
        mat1.SetFloat(ShaderID.Get("_GradientOffset"), .4f);
        mat2.SetFloat(ShaderID.Get("_GradientOffset"), .4f);
    }
}