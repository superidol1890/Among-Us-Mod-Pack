using System;

namespace TOHE;

[HarmonyPatch(typeof(HashRandom))]
class HashRandomPatch
{
    [HarmonyPatch(nameof(HashRandom.FastNext)), HarmonyPrefix]
    static bool FastNext_Prefix([HarmonyArgument(0)] int maxInt, ref int __result)
    {
        if (IRandom.Instance is HashRandomWrapper) return true;

        __result = IRandom.Instance.Next(maxInt);

        return false;
    }
    [HarmonyPatch(nameof(HashRandom.Next), new Type[] { typeof(int) }), HarmonyPrefix]
    static bool MaxNext_Prefix([HarmonyArgument(0)] int maxInt, ref int __result)
    {
        if (IRandom.Instance is HashRandomWrapper) return true;

        __result = IRandom.Instance.Next(maxInt);

        return false;
    }
    [HarmonyPatch(nameof(HashRandom.Next), new Type[] { typeof(int), typeof(int) }), HarmonyPrefix]
    static bool MinMaxNext_Prefix([HarmonyArgument(0)] int minInt, [HarmonyArgument(1)] int maxInt, ref int __result)
    {
        if (IRandom.Instance is HashRandomWrapper) return true;

        __result = IRandom.Instance.Next(minInt, maxInt);

        return false;
    }
}