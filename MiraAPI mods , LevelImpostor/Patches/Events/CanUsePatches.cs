using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;

namespace MiraAPI.Patches.Events;

[HarmonyPatch]
public static class CanUsePatches
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Console), nameof(Console.CanUse));
        yield return AccessTools.Method(typeof(MapConsole), nameof(MapConsole.CanUse));
        yield return AccessTools.Method(typeof(SystemConsole), nameof(SystemConsole.CanUse));
        yield return AccessTools.Method(typeof(Ladder), nameof(Ladder.CanUse));
        yield return AccessTools.Method(typeof(PlatformConsole), nameof(PlatformConsole.CanUse));
        yield return AccessTools.Method(typeof(OpenDoorConsole), nameof(OpenDoorConsole.CanUse));
        yield return AccessTools.Method(typeof(DoorConsole), nameof(DoorConsole.CanUse));
        yield return AccessTools.Method(typeof(OptionsConsole), nameof(OptionsConsole.CanUse));
        yield return AccessTools.Method(typeof(ZiplineConsole), nameof(ZiplineConsole.CanUse));
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool CanUsePatch(Il2CppSystem.Object __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        canUse = couldUse = false;

        IUsable usable = __instance.Cast<IUsable>();
        if (usable != null)
        {
            var @event = new PlayerCanUseEvent(__instance.Cast<IUsable>());
            MiraEventManager.InvokeEvent(@event);

            return !@event.IsCancelled;
        }

        return true;
    }
}
