using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace MiraAPI.Patches.Events;

/// <summary>
/// General patches for the KillAnimation class.
/// </summary>
[HarmonyPatch]
public static class KillAnimationPatches
{
    /// <summary>
    /// Used to trigger the <see cref="AfterMurderEvent"/>.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The target.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    public static void KillAnimPostfix(PlayerControl source, PlayerControl target)
    {
        var afterMurderEvent = new AfterMurderEvent(source, target);
        MiraEventManager.InvokeEvent(afterMurderEvent);
    }
}
