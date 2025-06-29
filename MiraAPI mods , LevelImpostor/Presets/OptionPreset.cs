using System.Linq;
using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Presets;

/// <summary>
/// Represents a preset of game options that can be applied to the game.
/// </summary>
public class OptionPreset
{
    /// <summary>
    /// Gets the name of the preset.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the plugin associated with the preset.
    /// </summary>
    public MiraPluginInfo Plugin { get; }

    /// <summary>
    /// Gets the configuration file of the plugin associated with the preset.
    /// </summary>
    public ConfigFile PluginConfig { get; }

    /// <summary>
    /// Gets the configuration file for the preset.
    /// </summary>
    public ConfigFile PresetConfig { get; }

    /// <summary>
    /// Gets or sets the button associated with the preset in the UI.
    /// </summary>
    public GameObject? PresetButton { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionPreset"/> class with the specified name and configuration file.
    /// </summary>
    /// <param name="name">>The name of the preset.</param>
    /// <param name="plugin">The plugin associated with the preset.</param>
    /// <param name="presetConfig">>The configuration file for the preset.</param>
    public OptionPreset(string name, MiraPluginInfo plugin, ConfigFile presetConfig)
    {
        Name = name;
        Plugin = plugin;
        PluginConfig = plugin.PluginConfig;
        PresetConfig = presetConfig;
    }

    /// <summary>
    /// Loads the preset by applying the values from the preset configuration to the plugin configuration.
    /// </summary>
    public void LoadPreset()
    {
        PresetConfig.Reload();
        foreach (var option in Plugin.InternalOptions)
        {
            option.LoadFromPreset(PresetConfig);
        }

        ModdedOptionsManager.SyncAllOptions();

        foreach (var role in Plugin.InternalRoles.Values.OfType<ICustomRole>())
        {
            role.LoadFromPreset(PresetConfig);
        }

        CustomRoleManager.SyncAllRoleSettings();
    }
}
