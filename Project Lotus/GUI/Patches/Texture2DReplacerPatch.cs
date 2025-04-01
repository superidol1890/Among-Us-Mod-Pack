using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Runtime;
using Lotus.Extensions;
using Lotus.Logging;
using UnityEngine;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.GUI.Patches;

public class Texture2DReplacerPatch
{
    public static readonly string[] ReplacedGraphics =
    [
        "GameSettingReworkAssets"
    ];

    public const string StartPath = "Lotus.assets.Replaced.";

    public static void CheckForTextures()
    {
        Resources.FindObjectsOfTypeAll(Il2CppType.Of<Texture2D>())
            .ForEach(obj =>
            {
                if (!obj.TryCast(out Texture2D texture)) return;
                if (texture == null) return;
                if (!ReplacedGraphics.Contains(texture.name)) return;
                DevLogger.Log($"Replacing {texture.name}.");

                using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(StartPath + texture.name + ".png");
                if (stream == null)
                {
                    DevLogger.Log($"Cannot replace texture! Null stream. Path: {StartPath + texture.name}.png");
                    return;
                }

                MemoryStream ms = new();
                try
                {
                    stream.CopyTo(ms);
                    texture.LoadImage(ms.ToArray(), false);
                    texture.filterMode = FilterMode.Trilinear;

                    texture.name += "_Replaced"; // Makes it so we don't override the same texture twice
                }
                catch (Exception)
                {
                    DevLogger.Log("Error replacing texture.");
                    throw;
                }
                finally
                {
                    ms.Close();
                }
            });
    }

    [QuickPostfix(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    private static void SettingsStartPostfix(GameSettingMenu __instance)
    {
        __instance.FindChild<SpriteRenderer>("LeftSideTint", true).gameObject.SetActive(false);
        CheckForTextures();
    }
}