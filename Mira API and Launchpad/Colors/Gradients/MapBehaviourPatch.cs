using HarmonyLib;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Features;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(MapBehaviour),nameof(MapBehaviour.Awake))]
public static class MapBehaviourPatch
{
    public static void Postfix(MapBehaviour __instance)
    {
        __instance.HerePoint.material = LaunchpadAssets.GradientMaterial.LoadAsset();
        
        __instance.HerePoint.gameObject.AddComponent<GradientColorComponent>().SetColor(PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId, PlayerControl.LocalPlayer.GetComponent<PlayerGradientData>().GradientColor);

        var mat = __instance.HerePoint.material;
        
        mat.SetFloat(ShaderID.Get("_GradientBlend"), 2);
        mat.SetFloat(ShaderID.Get("_GradientOffset"), .3f);
    }
}