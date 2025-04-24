using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace MiraAPI.Patches.Events;

[HarmonyPatch]
internal static class SubmergedSpawnGamePatch
{
    public static bool Prepare()
    {
        return ModCompatibility.SubmergedLoaded(out _);
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        if (ModCompatibility.SubmergedLoaded(out var subAss) &&
            subAss.GetType("Submerged.SpawnIn.SubmarineSelectSpawn") is { } selectSpawn)
        {
            yield return AccessTools.Method(selectSpawn, "OnDestroy");
        }
    }

    public static void Postfix()
    {
        var @event = new RoundStartEvent(false);
        MiraEventManager.InvokeEvent(@event);
    }
}
