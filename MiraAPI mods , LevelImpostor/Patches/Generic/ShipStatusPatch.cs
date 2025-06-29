using HarmonyLib;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Options;
using LaunchpadReloaded.Utilities;
using MiraAPI.GameOptions;
using UnityEngine;

namespace LaunchpadReloaded.Patches.Generic;

[HarmonyPatch(typeof(ShipStatus))]
public static class ShipStatusPatch
{
    /// <summary>
    /// Create nodes on map load.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShipStatus.Awake))]
    public static void ShipStatusBeginPostfix(ShipStatus __instance)
    {
        var nodesParent = new GameObject("Nodes");
        nodesParent.transform.SetParent(__instance.transform);

        var nodePositions = HackerUtilities.MapNodePositions[__instance.Type];
        if (__instance.TryCast<AirshipStatus>())
        {
            nodePositions = HackerUtilities.AirshipPositions;
        }

        for (var i = 0; i < nodePositions.Length; i++)
        {
            var nodePos = nodePositions[i];
            __instance.CreateNode(i, nodesParent.transform, nodePos);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShipStatus.StartMeeting))]
    public static void DisableFrozenBodyDeletionPatch()
    {
        foreach (var body in DeadBodyCacheComponent.GetFrozenBodies())
        {
            body.body.hideFlags = HideFlags.DontSave;
        }
    }

    /// <summary>
    /// Disable the meeting teleportation if option is enabled
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShipStatus.SpawnPlayer))]
    public static bool ShipStatusSpawnPlayerPrefix([HarmonyArgument(2)] bool initialSpawn)
    {
        if (TutorialManager.InstanceExists)
        {
            return true;
        }

        return initialSpawn || !OptionGroupSingleton<GeneralOptions>.Instance.DisableMeetingTeleport;
    }
}
