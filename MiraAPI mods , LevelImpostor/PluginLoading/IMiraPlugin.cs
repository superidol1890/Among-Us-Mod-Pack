using BepInEx.Configuration;

namespace MiraAPI.PluginLoading;

/// <summary>
/// The interface that all Mira plugins must implement.
/// </summary>
public interface IMiraPlugin
{
    /// <summary>
    /// Gets the name to display on the options menu.
    /// </summary>
    string OptionsTitleText { get; }

    /// <summary>
    /// Gets the BepInEx configuration file for the plugin.
    /// </summary>
    /// <returns>The BepInEx configuration file for the plugin.</returns>
    public ConfigFile GetConfigFile();
}
