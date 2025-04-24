using HarmonyLib;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Features;
using LaunchpadReloaded.Features.Managers;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadReloaded.Patches.Colors.Gradients;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
public static class PlayerVoteIconPatch
{
    public static bool Prefix(MeetingHud __instance, NetworkedPlayerInfo voterPlayer, int index, Transform parent)
    {
        if (!GradientManager.TryGetColor(voterPlayer.PlayerId, out var gradient))
        {
            return true;
        }
        
        var spriteRenderer = Object.Instantiate(__instance.PlayerVotePrefab, parent, true);
        if (!spriteRenderer.TryGetComponent<GradientColorComponent>(out var gradientComp))
        {
            gradientComp = spriteRenderer.gameObject.AddComponent<GradientColorComponent>();
        }

        spriteRenderer.material = LaunchpadAssets.MaskedGradientMaterial.LoadAsset();
        if (GameManager.Instance.LogicOptions.GetAnonymousVotes())
        {
            gradientComp.SetColor(Palette.DisabledGrey, Palette.DisabledGrey);
        }
        else
        {
            gradientComp.SetColor(voterPlayer.DefaultOutfit.ColorId, gradient);
        }
        
        gradientComp.mat.SetFloat(ShaderID.Get("GradientBlend"), 2);
        gradientComp.mat.SetFloat(ShaderID.Get("GradientOffset"), .3f);
        
        spriteRenderer.transform.localScale = Vector3.zero;
        var component = parent.GetComponent<PlayerVoteArea>();
        if (component != null)
        {
            spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
        }
        __instance.StartCoroutine(Effects.Bloop(index * 0.3f, spriteRenderer.transform));
        parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);

        return false;
    }
}