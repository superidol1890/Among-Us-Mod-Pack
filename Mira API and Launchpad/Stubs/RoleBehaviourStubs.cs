using System.Runtime.CompilerServices;
using HarmonyLib;
using Il2CppSystem.Text;

namespace MiraAPI.Patches.Stubs;

/// <summary>
/// Stub methods for the RoleBehaviour class. Needed because of Il2Cpp limitations on injected classes.
/// </summary>
[HarmonyPatch]
public static class RoleBehaviourStubs
{
    /// <summary>
    /// Stub method for RoleBehaviour.Initialize.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="player">The PlayerControl to initialize.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.Initialize))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Initialize(RoleBehaviour instance, PlayerControl player)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.AdjustTasks.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="player">The PlayerControl to adjust tasks for.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.AdjustTasks))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AdjustTasks(RoleBehaviour instance, PlayerControl player)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.AppendTaskHint.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="taskStringBuilder">The StringBuilder to append the task hint to.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.AppendTaskHint), typeof(StringBuilder))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AppendTaskHint(RoleBehaviour instance, StringBuilder taskStringBuilder)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.CanUse.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="console">The IUsable console to check.</param>
    /// <returns>Whether the console can be used.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.CanUse))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool CanUse(RoleBehaviour instance, IUsable console)
    {
        return false;
    }

    /// <summary>
    /// Stub method for RoleBehaviour.Deinitialize.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="targetPlayer">The PlayerControl to deinitialize for.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.Deinitialize))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Deinitialize(RoleBehaviour instance, PlayerControl targetPlayer)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.DidWin.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="gameOverReason">The reason for game over.</param>
    /// <returns>Whether the role won.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.DidWin))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool DidWin(RoleBehaviour instance, GameOverReason gameOverReason)
    {
        return false;
    }

    /// <summary>
    /// Stub method for RoleBehaviour.FindClosestTarget.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <returns>The closest target PlayerControl.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.FindClosestTarget))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static PlayerControl FindClosestTarget(RoleBehaviour instance)
    {
        return null!;
    }

    /// <summary>
    /// Stub method for RoleBehaviour.GetAbilityDistance.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <returns>The ability distance.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.GetAbilityDistance))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float GetAbilityDistance(RoleBehaviour instance)
    {
        return 0;
    }

    /// <summary>
    /// Stub method for RoleBehaviour.IsValidTarget.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="target">The NetworkedPlayerInfo to validate.</param>
    /// <returns>Whether the target is valid.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.IsValidTarget))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsValidTarget(RoleBehaviour instance, NetworkedPlayerInfo target)
    {
        return false;
    }

    /// <summary>
    /// Stub method for RoleBehaviour.OnDeath.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="reason">The reason for death.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnDeath))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnDeath(RoleBehaviour instance, DeathReason reason)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.OnMeetingStart.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnMeetingStart))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnMeetingStart(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.OnVotingComplete.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnVotingComplete))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnVotingComplete(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.SetCooldown.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.SetCooldown))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetCooldown(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.SetPlayerTarget.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="target">The PlayerControl target to set.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.SetPlayerTarget))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetPlayerTarget(RoleBehaviour instance, PlayerControl target)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.SetUsableTarget.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="target">The IUsable target to set.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.SetUsableTarget))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetUsableTarget(RoleBehaviour instance, IUsable target)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.SpawnTaskHeader.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    /// <param name="playerControl">The PlayerControl to spawn the task header for.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.SpawnTaskHeader))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SpawnTaskHeader(RoleBehaviour instance, PlayerControl playerControl)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for RoleBehaviour.UseAbility.
    /// </summary>
    /// <param name="instance">The RoleBehaviour object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.UseAbility))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void UseAbility(RoleBehaviour instance)
    {
        // nothing needed
    }
}
