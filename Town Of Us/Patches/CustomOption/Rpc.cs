using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.CustomOption
{
    public static class Rpc
    {
        public static IEnumerator SendRpc(CustomOption optionn = null, int RecipientId = -1)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            List<CustomOption> options;
            if (optionn != null)
            {
                options = new List<CustomOption> { optionn };
            }
            else
            {
                options = CustomOption.AllOptions;
            }

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.SyncCustomSettings, SendOption.Reliable, RecipientId);

            foreach (var option in options)
            {
                if (option.Type == CustomOptionType.Header) continue;

                if (writer.Position > 1000) {
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SyncCustomSettings, SendOption.Reliable, RecipientId);
                }
                writer.WritePacked(option.ID);

                switch (option.Type)
                {
                    case CustomOptionType.Toggle:
                        writer.Write((bool)option.Value);
                        break;
                    case CustomOptionType.Number:
                        switch ((option as CustomNumberOption).IntSafe)
                        {
                            case true:
                                writer.WritePacked((int)(float)option.Value);
                                break;
                            case false:
                                writer.Write((float)option.Value);
                                break;
                        }
                        break;
                    case CustomOptionType.String:
                        writer.WritePacked((int)option.Value);
                        break;
                }
            }

            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void ReceiveRpc(MessageReader reader, bool AllOptions)
        {
            PluginSingleton<TownOfUs>.Instance.Log.LogInfo($"Options received - {reader.BytesRemaining} bytes");
            while (reader.BytesRemaining > 0)
            {
                var id = reader.ReadPackedInt32();
                var customOption =
                    CustomOption.AllOptions.FirstOrDefault(option =>
                        option.ID == id); // Works but may need to change to gameObject.name check
                var type = customOption?.Type;
                object value = null;

                switch (type)
                {
                    case CustomOptionType.Toggle:
                        value = reader.ReadBoolean();
                        break;
                    case CustomOptionType.Number:
                        switch ((customOption as CustomNumberOption).IntSafe)
                        {
                            case true:
                                value = (float)reader.ReadPackedInt32();
                                break;
                            case false:
                                value = reader.ReadSingle();
                                break;
                        }
                        break;
                    case CustomOptionType.String:
                        value = reader.ReadPackedInt32();
                        break;
                }

                customOption?.Set(value, Notify: !AllOptions);

                if (LobbyInfoPane.Instance.LobbyViewSettingsPane.gameObject.activeSelf)
                {
                    var panels = GameObject.FindObjectsOfType<ViewSettingsInfoPanel>();
                    foreach (var panel in panels)
                    {
                        if (panel.titleText.text == customOption.Name && customOption.Type != CustomOptionType.Header)
                        {
                            panel.settingText.text = customOption.ToString();
                        }
                    }
                }
            }
        }
    }
}