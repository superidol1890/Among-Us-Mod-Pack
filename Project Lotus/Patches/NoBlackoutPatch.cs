using HarmonyLib;

namespace Lotus.Patches;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
class DontBlackoutPatch
{
    public static void Postfix(ref bool __result)
    {
        __result = false;
    }
}