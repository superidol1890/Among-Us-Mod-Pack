using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ModPack;

[BepInAutoPlugin("gg.Mod.Pack")]
[BepInProcess("Among Us.exe")]
public partial class ModPackPlugin : BasePlugin
{
    private static StringNames _helloStringName;

    public override void Load()
    {
        Credits.Register<ModPackPlugin>(Credits.AlwaysShow);

        this.AddComponent<ModPackComponent>();

        _helloStringName = CustomStringName.CreateAndRegister("Hello!");
        LocalizationManager.Register(new ExampleLocalizationProvider());
    }

    [RegisterInIl2Cpp]
    public class ModPackComponent : MonoBehaviour
    {
        [HideFromIl2Cpp]
        public DragWindow TestWindow { get; }

        public ModPackComponent(IntPtr ptr) : base(ptr)
        {
            TestWindow = new DragWindow(new (60, 20, 0, 0), "ModPack", () =>
            {
                if (GUILayout.Button("Log CustomStringName"))
                {
                    Logger<ModPackPlugin>.Info(TranslationController.Instance.GetString(_helloStringName));
                }

                if (GUILayout.Button("Log localized string"))
                {
                    Logger<ModPackPlugin>.Info(TranslationController.Instance.GetString((StringNames) 1337));
                }

                if (AmongUsClient.Instance && PlayerControl.LocalPlayer)
                {
                    if (GUILayout.Button("Send ModPackRpc"))
                    {
                        var playerName = PlayerControl.LocalPlayer.Data.PlayerName;
                        Rpc<ModPackRpc>.Instance.Send(new ModPackRpc.Data($"Send: from {playerName}"), ackCallback: () =>
                        {
                            Logger<ModPackPlugin>.Info("Got an acknowledgement for modpack rpc");
                        });

                        if (!AmongUsClient.Instance.AmHost)
                        {
                            Rpc<ModPackRpc>.Instance.SendTo(AmongUsClient.Instance.HostId, new ModPackRpc.Data($"SendTo: from {playerName} to host"));
                        }
                    }

                    if (GUILayout.Button("Send MethodModPackRpc"))
                    {
                        RpcSay(PlayerControl.LocalPlayer, "Hello from method rpc", Random.value, PlayerControl.LocalPlayer);
                    }
                }
            })
            {
                Enabled = false,
            };
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TestWindow.Enabled = !TestWindow.Enabled;
            }
        }

        private void OnGUI()
        {
            TestWindow.OnGUI();
        }
    }

    [MethodRpc((uint) CustomRpcCalls.MethodRpcExample)]
    public static void RpcSay(PlayerControl player, string text, float number, PlayerControl testPlayer)
    {
        Logger<ModPackPlugin>.Info($"{player.Data.PlayerName} text: {text} number: {number} testPlayer: {testPlayer.NetId}");
    }
}
