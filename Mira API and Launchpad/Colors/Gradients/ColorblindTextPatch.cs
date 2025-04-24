using HarmonyLib;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Utilities;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.GetColorBlindText))]
public static class ColorblindTextPatch
{
    public static bool Prefix(CosmeticsLayer __instance, ref string __result)
    {
        if (!__instance.TryGetComponent(out PlayerGradientData comp) &&
            !__instance.transform.parent.TryGetComponent(out comp))
        {
            return true;
        }

        var plr = GameData.Instance.GetPlayerById(comp.playerId);

        if (plr.IsHacked())
        {
            __result = "???";
            return false;
        }

        if (!comp.GradientEnabled)
        {
            return true;
        }

        var defaultColor = Helpers.FirstLetterToUpper(Palette.GetColorName(__instance.ColorId).ToLower());
        var gradientColor = Helpers.FirstLetterToUpper(Palette.GetColorName(comp.GradientColor).ToLower());

        if (defaultColor == gradientColor || gradientColor == "???")
        {
            __result = defaultColor;
            return false;
        }

        if (!__instance.GetComponentInParent<PlayerVoteArea>() &&
            !__instance.GetComponentInParent<PlayerControl>() &&
            !__instance.GetComponentInParent<ShapeshifterPanel>())
        {
            __result = $"{gradientColor}\n{defaultColor}";

        }
        else
        {
            __result = $"{gradientColor}-{defaultColor}";
        }

        return false;
    }
}