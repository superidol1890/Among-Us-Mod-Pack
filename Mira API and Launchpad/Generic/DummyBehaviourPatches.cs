using AmongUs.GameOptions;
using HarmonyLib;
using LaunchpadReloaded.Features;

namespace LaunchpadReloaded.Patches.Generic;

[HarmonyPatch(typeof(DummyBehaviour))]
public static class DummyBehaviourPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(DummyBehaviour.Start))]
    public static void DummyStartPatch(DummyBehaviour __instance)
    {
        __instance.myPlayer.RpcSetRole(RoleTypes.Crewmate);

        if (LaunchpadSettings.Instance?.UniqueDummies.Enabled == true)
        {
            __instance.myPlayer.RpcSetName(AccountManager.Instance.GetRandomName());
        }
    }
}