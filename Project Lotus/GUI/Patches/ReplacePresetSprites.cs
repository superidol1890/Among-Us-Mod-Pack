using HarmonyLib;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.GUI.Patches;

public class ReplacePresetSprites
{
    public const string Prefix = "Presets.";

    [QuickPostfix(typeof(GameSettingMenu), nameof(GameSettingMenu.Start), Priority.Low)]
    public static void SettingsStartPostfix(GameSettingMenu __instance)
    {
        __instance.FindChild<SpriteRenderer>("DownArrow/Inactive").sprite = AssetLoader.LoadLotusSprite(Prefix + "Arrow.png", 200f, true);
        __instance.FindChild<SpriteRenderer>("AddPreset/Inactive").sprite = AssetLoader.LoadLotusSprite(Prefix + "Plus.png", 200f, true);
        __instance.FindChild<SpriteRenderer>("UpArrow/Inactive").sprite = AssetLoader.LoadLotusSprite(Prefix + "Arrow.png", 200f, true);
        __instance.FindChild<SpriteRenderer>("Trash/Inactive").sprite = AssetLoader.LoadLotusSprite(Prefix + "Trash.png", 220f, true);
    }
}