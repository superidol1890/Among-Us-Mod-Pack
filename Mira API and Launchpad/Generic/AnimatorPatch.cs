using HarmonyLib;
using LaunchpadReloaded.Roles.Impostor;
using UnityEngine;

namespace LaunchpadReloaded.Patches.Generic;

[HarmonyPatch]
public static class AnimatorPatch
{
    private const float GlobalSpeedMultiplier = 0.5f;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Animator), nameof(Animator.Update))]
    public static void SetSpeedPatch(Animator __instance)
    {
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role is HitmanRole
            {
                inDeadlockMode: true
            })
        {
            __instance.speed *= GlobalSpeedMultiplier;
        }
    }
}