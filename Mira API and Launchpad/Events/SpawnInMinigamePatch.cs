using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace MiraAPI.Patches.Events;

[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]
internal static class SpawnInMinigamePatch
{
    public static void Postfix()
    {
        var @event = new RoundStartEvent(false);
        MiraEventManager.InvokeEvent(@event);
    }
}
