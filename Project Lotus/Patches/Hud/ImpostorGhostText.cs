using HarmonyLib;
using Il2CppSystem;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections.Generic;

namespace Lotus.Patches.Hud;

[HarmonyPatch(typeof(ImpostorGhostRole), nameof(ImpostorGhostRole.SpawnTaskHeader))]
class ImpostorGhostText
{
    public static bool Prefix(ImpostorGhostRole __instance, [HarmonyArgument(0)] PlayerControl playerControl)
    {
        ImportantTextTask textTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        textTask.Text = "\n" + DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostImpostor, new Il2CppReferenceArray<Object>(new List<Object>().ToArray()));
        return false;
    }
}