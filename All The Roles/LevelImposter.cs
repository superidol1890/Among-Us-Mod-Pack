using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.Shop;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace LevelImposter;

[BepInAutoPlugin(ID, "LevelImposter")]
[BepInDependency(ModCompatibility.REACTOR_ID)]
[BepInDependency(ModCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompatibility.TOU_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompatibility.TOR_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompatibility.REW_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
[BepInProcess("Among Us.exe")]
public partial class LevelImposter : BasePlugin
{
    public const string ID = "com.DigiWorm.LevelImposter";

    public Harmony Harmony { get; } = new(ID);

    public static string DisplayVersion { get; } =
        Version.Contains('+') ? Version.Substring(0, Version.IndexOf('+')) : Version;

    public override void Load()
    {
        // Init Subsystems
        LILogger.Init();
        MapFileAPI.Init();
        FileCache.Init();
        ModCompatibility.Init();

        // IUsable Interface
        RegisterTypeOptions usableInterface = new()
        {
            Interfaces = new Il2CppInterfaceCollection(new[]
            {
                typeof(IUsable)
            })
        };

        // Inject MonoBehaviours
        ClassInjector.RegisterTypeInIl2Cpp<LIShipStatus>();
        ClassInjector.RegisterTypeInIl2Cpp<LIStar>();
        ClassInjector.RegisterTypeInIl2Cpp<LIFloat>();
        ClassInjector.RegisterTypeInIl2Cpp<LIScroll>();
        ClassInjector.RegisterTypeInIl2Cpp<LITeleporter>();
        ClassInjector.RegisterTypeInIl2Cpp<LITriggerArea>();
        ClassInjector.RegisterTypeInIl2Cpp<TriggerAnim>();
        ClassInjector.RegisterTypeInIl2Cpp<LIDeathArea>();
        ClassInjector.RegisterTypeInIl2Cpp<LIShakeArea>();
        ClassInjector.RegisterTypeInIl2Cpp<LITriggerSpawnable>();
        ClassInjector.RegisterTypeInIl2Cpp<MinigameSprites>();
        ClassInjector.RegisterTypeInIl2Cpp<LagLimiter>();
        ClassInjector.RegisterTypeInIl2Cpp<GIFAnimator>();
        ClassInjector.RegisterTypeInIl2Cpp<TriggerSoundPlayer>();
        ClassInjector.RegisterTypeInIl2Cpp<TriggerConsole>(usableInterface);
        ClassInjector.RegisterTypeInIl2Cpp<MapObjectData>();
        ClassInjector.RegisterTypeInIl2Cpp<EditableLadderConsole>();
        ClassInjector.RegisterTypeInIl2Cpp<LIExileController>();
        ClassInjector.RegisterTypeInIl2Cpp<LIPhysicsObject>();

        ClassInjector.RegisterTypeInIl2Cpp<AssetDB>();

        ClassInjector.RegisterTypeInIl2Cpp<HTTPHandler>();
        ClassInjector.RegisterTypeInIl2Cpp<RandomOverlay>();
        ClassInjector.RegisterTypeInIl2Cpp<MapBanner>();
        ClassInjector.RegisterTypeInIl2Cpp<ShopManager>();
        ClassInjector.RegisterTypeInIl2Cpp<ShopTabs>();
        ClassInjector.RegisterTypeInIl2Cpp<Spinner>();
        ClassInjector.RegisterTypeInIl2Cpp<LoadingBar>();
        ClassInjector.RegisterTypeInIl2Cpp<LobbyVersionTag>();
        ClassInjector.RegisterTypeInIl2Cpp<LobbyConsole>(usableInterface);

        // Patch Methods
        Harmony.PatchAll();
        LILogger.Msg("LevelImposter Initialized.");
    }
}