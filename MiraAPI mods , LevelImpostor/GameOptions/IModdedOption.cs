using System;
using BepInEx.Configuration;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using UnityEngine;

namespace MiraAPI.GameOptions;

/// <summary>
/// Interface for modded options.
/// </summary>
public interface IModdedOption
{
    /// <summary>
    /// Gets the unique identifier for the option.
    /// </summary>
    uint Id { get; }

    /// <summary>
    /// Gets the title of the option.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the StringName for the option, used for localization.
    /// </summary>
    StringNames StringName { get; }

    /// <summary>
    /// Gets or sets the MiraPlugin that created this option.
    /// </summary>
    IMiraPlugin? ParentMod { get; set; }

    /// <summary>
    /// Gets the game setting data for the option.
    /// </summary>
    BaseGameSetting Data { get; }

    /// <summary>
    /// Gets the OptionBehaviour object of the option.
    /// </summary>
    OptionBehaviour? OptionBehaviour { get; }

    /// <summary>
    /// Gets or sets the visibility function for the option.
    /// </summary>
    Func<bool> Visible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the option should be included with presets.
    /// </summary>
    bool IncludeInPreset { get; set; }

    /// <summary>
    /// Gets or sets the ConfigDefinition for the option, used for BepInEx configuration.
    /// </summary>
    ConfigDefinition? ConfigDefinition { get; set; }

    /// <summary>
    /// Creates the option behaviour for the modded option.
    /// </summary>
    /// <param name="toggleOpt">The ToggleOption template.</param>
    /// <param name="numberOpt">The NumberOption template.</param>
    /// <param name="stringOpt">The StringOption template.</param>
    /// <param name="container">>The Transform container for the option.</param>
    /// <returns>>The created OptionBehaviour object.</returns>
    OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container);

    /// <summary>
    /// Gets the value as a float.
    /// </summary>
    /// <returns>>The value of the option as a float.</returns>
    float GetFloatData();

    /// <summary>
    /// Gets the NetData for the option, used for network synchronization.
    /// </summary>
    /// <returns>>Returns the NetData object for the option.</returns>
    NetData GetNetData();

    /// <summary>
    /// Handles incoming network data for the option.
    /// </summary>
    /// <param name="data">>The byte array representing the network data.</param>
    void HandleNetData(byte[] data);

    /// <summary>
    /// Saves the option to a preset configuration file.
    /// </summary>
    /// <param name="presetConfig">The ConfigFile representing the preset configuration.</param>
    /// <param name="saveDefault">Indicates whether to save the default value instead of the current value.</param>
    void SaveToPreset(ConfigFile presetConfig, bool saveDefault = false);

    /// <summary>
    /// Binds the option to a configuration file.
    /// </summary>
    /// <param name="config">The ConfigFile to bind the option to.</param>
    void Bind(ConfigFile config);

    /// <summary>
    /// Loads the option from a preset configuration file, applying the values to the option's configuration.
    /// </summary>
    /// <param name="presetConfig">The ConfigFile representing the preset configuration.</param>
    void LoadFromPreset(ConfigFile presetConfig);
}
