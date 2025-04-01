// using HarmonyLib;
// using Il2CppInterop.Runtime.InteropTypes.Arrays;
// using UnityEngine;
// using VentLib.Logging;
// using AmongUs.GameOptions;
// using Lotus.Utilities;
// using VentLib.Utilities;

// namespace VentLib.Options.Game.Patches;

// [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
// public static class GameSettingsStartPatch
// {
//     public static void Postfix(GameSettingMenu __instance)
//     {
//         Async.Schedule(() =>
//         {
//             Transform background = __instance.transform.Find("Background");
//             GameObject plBackground = new("PLBackground");
//             plBackground.transform.localPosition = new Vector3(background.localPosition.x, background.localPosition.y, background.localPosition.z);
//             var spriteRenderer = plBackground.AddComponent<SpriteRenderer>();
//             spriteRenderer.sprite = AssetLoader.LoadSprite("Lotus.assets.PLBackground-Upscale.png", 250f);
//             plBackground.transform.parent = background.parent;
//             background.gameObject.SetActive(false);
//         }, 0.05f);
//     }
// }
// // did not work so REMOVED, maybe I'll come back to this