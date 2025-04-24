using HarmonyLib;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Features.Managers;
using UnityEngine;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(int), typeof(Renderer))]
public static class PlayerMaterialPatch
{
    public static void Postfix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer renderer)
    {
        if (renderer.GetComponentInParent<HatParent>() && !renderer.GetComponentInParent<CosmeticsLayer>())
        {
            return;
        }

        var color2 = GradientManager.LocalGradientId;
        var enabled = true;

        if (PlayerCustomizationMenu.Instance && PlayerTabPatches.SelectGradient)
        {
            var plrTab = PlayerCustomizationMenu.Instance.GetComponentInChildren<PlayerTab>();
            if (plrTab)
            {
                color2 = plrTab.currentColor;
            }
        }

        if (renderer.GetComponentInParent<PlayerGradientData>() is {} comp && comp)
        {
            color2 = comp.GradientColor;
            enabled = comp.GradientEnabled;
        }

        /*if (GameData.Instance)
        {
            byte id = 255;
            if (renderer.GetComponentInParent<PlayerControl>())
            {d
                id = renderer.GetComponentInParent<PlayerControl>().PlayerId;
            }

            if (renderer.GetComponent<DeadBody>())
            {
                id = renderer.GetComponent<DeadBody>().ParentId;
            }

            if (renderer.GetComponent<PetBehaviour>())
            {
                var pet = renderer.GetComponent<PetBehaviour>();
                if (pet.TargetPlayer)
                {
                    id = pet.TargetPlayer.PlayerId;
                }
            }

            if (id != 255 && GradientManager.TryGetColor(id, out var color))
            {
                color2 = color;
            }
        }*/



        var gradColor = renderer.GetComponent<GradientColorComponent>();
        if (!gradColor)
        {
            gradColor = renderer.gameObject.AddComponent<GradientColorComponent>();
        }

        gradColor.SetColor(colorId, enabled ? color2 : colorId);

    }
}