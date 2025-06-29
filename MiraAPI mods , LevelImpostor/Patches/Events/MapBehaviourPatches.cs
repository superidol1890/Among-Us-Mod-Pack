using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Map;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Patch for map related MiraEvents.
/// </summary>
[HarmonyPatch]
public static class MapBehaviourPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    public static bool MapShowSabotagePatch(MapBehaviour __instance)
    {
        var @event = new PlayerOpenSabotageEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }
}
