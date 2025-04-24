using HarmonyLib;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(LongBoiPlayerBody),nameof(LongBoiPlayerBody.Start))]
public static class LongBoiPatch
{
    public static void Postfix(LongBoiPlayerBody __instance)
    {
        __instance.cosmeticLayer.currentBodySprite.BodySprite.material.SetFloat(ShaderID.Get("_GradientOffset"), -2);
        
        __instance.neckSprite.material.SetFloat(ShaderID.Get("_GradientOffset"), -2f);
        __instance.foregroundNeckSprite.material.SetFloat(ShaderID.Get("_GradientOffset"), -0.5f);
        
        __instance.headSprite.material.SetFloat(ShaderID.Get("_GradientOffset"), 2);
    }
}

[HarmonyPatch(typeof(CosmeticsLayer),nameof(CosmeticsLayer.EnsureInitialized))]
public static class HorsePatch
{
    public static void Postfix(CosmeticsLayer __instance, PlayerBodyTypes bt)
    {
        if (bt is not PlayerBodyTypes.Horse)
        {
            return;
        }

        __instance.currentBodySprite.BodySprite.material.SetFloat(ShaderID.Get("_GradientOffset"), 1.5f);
    }
}