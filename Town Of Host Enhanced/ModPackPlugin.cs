using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Localization;
using Reactor.Localization.Utilities;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ModPack;

[BepInAutoPlugin("AU.ModPack.api")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class ModPackPlugin : BasePlugin
{
    private static ModPackPluginManager? PluginManager { get; set; }

    public override void Load()
    {
       Harmony.PatchAll();

        ReactorCredits.Register("Mod Pack", Version, true, ReactorCredits.AlwaysShow);

        PluginManager = new PluginManager();
        PluginManager.Initialize();
    }
}