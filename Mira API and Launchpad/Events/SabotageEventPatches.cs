using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Map;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Used for patching sabotage/system related MiraEvents.
/// </summary>
[HarmonyPatch]
public static class SabotageEventPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), typeof(SystemTypes), typeof(byte))]
    public static bool ShipStatusUpdateSystemPrefix(ShipStatus __instance, SystemTypes systemType, byte amount)
    {
        var @event = new UpdateSystemEvent(systemType, PlayerControl.LocalPlayer, amount);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcCloseDoorsOfType))]
    public static bool ShipStatusCloseDoorsOfTypePrefix(ShipStatus __instance, SystemTypes type)
    {
        var @event = new CloseDoorsEvent(type);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }
}
