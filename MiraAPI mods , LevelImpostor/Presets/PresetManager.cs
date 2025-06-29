using System.IO;
using System.Linq;
using BepInEx.Configuration;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Presets;

/// <summary>
/// Provides functionality to manage and load game option presets for plugins.
/// </summary>
public static class PresetManager
{
    /// <summary>
    /// Gets the directory where the presets are stored.
    /// </summary>
    public static string PresetDirectory { get; } = Path.GetFullPath("mira_presets", Application.persistentDataPath);

    internal static void CreateDefaultPreset(MiraPluginInfo plugin)
    {
        var presetPath = Path.Combine(PresetDirectory, plugin.PluginId);
        if (!Directory.Exists(presetPath))
        {
            Directory.CreateDirectory(presetPath);
        }

        var presetConfig = new ConfigFile(Path.Combine(presetPath, "Default.cfg"), false);

        foreach (var option in plugin.InternalOptions.Where(x => x.IncludeInPreset))
        {
            option.SaveToPreset(presetConfig, true);
        }

        foreach (var role in plugin.InternalRoles.Values.OfType<ICustomRole>())
        {
            role.SaveToPreset(presetConfig);
        }

        presetConfig.Save();
    }

    /// <summary>
    /// Loads the presets for the specified plugin by reading the configuration files from the preset directory.
    /// </summary>
    /// <param name="plugin">>The plugin for which the presets should be loaded.</param>
    public static void LoadPresets(MiraPluginInfo plugin)
    {
        foreach (var btn in plugin.InternalPresets.Select(x=>x.PresetButton))
        {
            if (btn != null)
            {
                Object.DestroyImmediate(btn);
            }
        }

        plugin.InternalPresets.Clear();
        if (!Directory.Exists(PresetDirectory))
        {
            Directory.CreateDirectory(PresetDirectory);
        }

        var pluginPresetPath = Path.Combine(PresetDirectory, plugin.PluginId);
        if (!Directory.Exists(pluginPresetPath))
        {
            Directory.CreateDirectory(pluginPresetPath);
        }

        foreach (var file in Directory.GetFiles(pluginPresetPath, "*.cfg"))
        {
            var fileName = Path.GetFileName(file);
            Logger<MiraApiPlugin>.Info($"Loading preset file {fileName}");
            var presetName = Path.GetFileNameWithoutExtension(file);
            var presetConfig = new ConfigFile(file, false)
            {
                SaveOnConfigSet = false,
            };

            foreach (var option in plugin.InternalOptions.Where(x=>x.IncludeInPreset))
            {
                option.Bind(presetConfig);
            }

            foreach (var role in plugin.InternalRoles.Values.OfType<ICustomRole>().Where(x=>!x.Configuration.HideSettings))
            {
                role.BindConfig(presetConfig);
            }

            presetConfig.Save();

            plugin.InternalPresets.Add(new OptionPreset(presetName, plugin, presetConfig));
        }
        plugin.Presets = [..plugin.InternalPresets];
    }
}
