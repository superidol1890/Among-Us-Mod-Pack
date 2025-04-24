using HarmonyLib;
using MiraAPI.Utilities.Assets;

namespace MiraAPI.Patches.Menu;

/// <summary>
/// General MainMenuManage patches.
/// </summary>
[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatches
{
    /// <summary>
    /// A postifix on Awake to load all the addressables registered.
    /// </summary>
    [HarmonyPatch(nameof(MainMenuManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix()
    {
        AddressablesLoader.LoadAll();
    }
}
