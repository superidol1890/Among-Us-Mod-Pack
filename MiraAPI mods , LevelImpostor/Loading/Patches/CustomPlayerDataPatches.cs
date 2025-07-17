﻿using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Submerged.Enums;
using Submerged.Extensions;

namespace Submerged.Loading.Patches;

[HarmonyPatch]
public class CustomPlayerDataPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    public static void SendCustomDataPatch(PlayerControl __instance, ref CppIEnumerator __result)
    {
        __result = CoStartAndSendCustomData(__instance, __result).WrapToIl2Cpp();
    }

    private static IEnumerator CoStartAndSendCustomData(PlayerControl playerControl, CppIEnumerator original)
    {
        yield return original;

        if (!playerControl.AmOwner) yield break;
        bool mapLoaded = !AssetLoader.Errored;

        if (AmongUsClient.Instance.AmClient)
        {
            CustomPlayerData data = playerControl.gameObject.EnsureComponent<CustomPlayerData>();
            data.HasMap = mapLoaded;
        }

        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(playerControl.NetId, CustomRpcCalls.SetCustomData, SendOption.Reliable);
        messageWriter.Write(mapLoaded);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    [HarmonyPrefix]
    public static bool HandleCustomDataPatch(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (callId != CustomRpcCalls.SetCustomData) return true;

        CustomPlayerData data = __instance.gameObject.EnsureComponent<CustomPlayerData>();
        data.HasMap = reader.ReadBoolean();

        return false;
    }
}
