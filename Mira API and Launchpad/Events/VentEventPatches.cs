using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Patches to invoke vent related MiraEvents.
/// </summary>
[HarmonyPatch]
public static class VentEventPatches
{
    // necessary because Vent.Use is inlined in il2cpp.
    private static bool _showButtons;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    public static void VentUsePostfix(VentButton __instance)
    {
        __instance.currentTarget?.SetButtons(_showButtons);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcEnterVent))]
    public static bool PlayerPhysicsRpcEnterVentPrefix(PlayerPhysics __instance, int id)
    {
        var pc = __instance.myPlayer;
        var vent = ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == id);

        var @event = new EnterVentEvent(pc, vent);
        MiraEventManager.InvokeEvent(@event);

        if (pc.AmOwner)
        {
            _showButtons = !@event.IsCancelled;
        }
        return !@event.IsCancelled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
    public static bool PlayerPhysicsRpcExitVentPrefix(PlayerPhysics __instance, int id)
    {
        var pc = __instance.myPlayer;
        var vent = ShipStatus.Instance.AllVents.First(v => v.Id == id);

        var @event = new ExitVentEvent(pc, vent);
        MiraEventManager.InvokeEvent(@event);

        if (pc.AmOwner)
        {
            _showButtons = @event.IsCancelled;
        }
        return !@event.IsCancelled;
    }
}
