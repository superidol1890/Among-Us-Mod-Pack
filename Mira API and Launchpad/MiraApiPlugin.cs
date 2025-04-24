using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI;

/// <summary>
/// The main plugin class for Mira API.
/// </summary>
[BepInAutoPlugin("mira.api", "MiraAPI")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(ModCompatibility.SubmergedId, BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class MiraApiPlugin : BasePlugin
{
    /// <summary>
    /// Gets the branding Mira API color.
    /// </summary>
    public static Color MiraColor { get; } = new Color32(238, 154, 112, 255);

    /// <summary>
    /// Gets the default color for headers in the options menu.
    /// </summary>
    public static Color DefaultHeaderColor { get; } = new Color32(77, 77, 77, 255);

    private static MiraPluginManager? PluginManager { get; set; }
    internal Harmony Harmony { get; } = new(Id);

    /// <inheritdoc />
    public override void Load()
    {
        Harmony.PatchAll();

        ReactorCredits.Register("Mira API", Version, true, ReactorCredits.AlwaysShow);

        PluginManager = new MiraPluginManager();
        PluginManager.Initialize();
    }
}
