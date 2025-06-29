using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace LaunchpadReloaded.Patches;

//[HarmonyPatch]
public static class CrowdedModPatch
{
    public const string CrowdedId = "xyz.crowdedmods.crowdedmod";

    public static bool CrowdedLoaded([NotNullWhen(true)] out Assembly? assembly)
    {
        var result = IL2CPPChainloader.Instance.Plugins.TryGetValue(CrowdedId, out var plugin);
        assembly = result ? plugin?.Instance.GetType().Assembly : null;
        return result;
    }

    public static bool Prepare()
    {
        return CrowdedLoaded(out _);
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        if (CrowdedLoaded(out var assembly) && assembly.GetType("CrowdedMod.Patches.GenericPatches") is { } genericPatches)
        {
            yield return AccessTools.PropertyGetter(genericPatches, "ShouldDisableColorPatch");
        }
    }

    // ReSharper disable once InconsistentNaming
    public static void Postfix(ref bool __result)
    {
        __result = true;

    }
}