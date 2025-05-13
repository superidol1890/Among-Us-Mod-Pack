using HarmonyLib;
using Hazel;
using Lotus.Extensions;
using static InnerNet.InnerNetClient;

namespace Lotus.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.Serialize))]
class GameManagerSerializeFix
{
    public static bool Prefix(GameManager __instance, [HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] bool initialState, ref bool __result)
    {
        bool flag = false;
        for (int index = 0; index < __instance.LogicComponents.Count; ++index)
        {
            GameLogicComponent logicComponent = __instance.LogicComponents[index];

            if (!initialState && AmongUsClient.Instance.GameState is GameStates.Started && logicComponent.TryCast(out LogicOptions _))
            {
                logicComponent.ClearDirtyFlag();
                continue;
            }

            if (initialState || logicComponent.IsDirty)
            {
                flag = true;
                writer.StartMessage((byte)index);
                logicComponent.Serialize(writer, initialState);
                writer.EndMessage();
                logicComponent.ClearDirtyFlag();
            }
        }
        __instance.ClearDirtyBits();
        __result = flag;
        return false;
    }
}
