// from https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/3676f644e39612b8fe7ea6ddfd0b46a643f08dba/Modules/DelayNetworkedData.cs

using Hazel;
using System;
using InnerNet;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using VentLib.Utilities.Harmony.Attributes;
using System.Linq;
using VentLib.Utilities;

namespace Lotus.Network;

public class DelayNetworkedData
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(DelayNetworkedData));

    [QuickPrefix(typeof(InnerNetClient), nameof(InnerNetClient.SendInitialData))]
    public static bool SendInitialDataPrefix(InnerNetClient __instance, int clientId)
    {
        if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return true;
        // We make sure other stuffs like playercontrol and Lobby behavior is spawned properly
        // Then we spawn networked data for new clients
        MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
        messageWriter.StartMessage(6);
        messageWriter.Write(__instance.GameId);
        messageWriter.WritePacked(clientId);
        Il2CppSystem.Collections.Generic.List<InnerNetObject> obj = __instance.allObjects;
        lock (obj)
        {
            HashSet<GameObject> hashSet = [];
            for (int i = 0; i < __instance.allObjects.Count; i++)
            {
                InnerNetObject innerNetObject = __instance.allObjects[i];
                if (innerNetObject && (innerNetObject.OwnerId != -4 || __instance.AmModdedHost) && hashSet.Add(innerNetObject.gameObject))
                {
                    GameManager gameManager = innerNetObject as GameManager;
                    if (gameManager != null)
                    {
                        __instance.SendGameManager(clientId, gameManager);
                    }
                    else
                    {
                        if (innerNetObject is not NetworkedPlayerInfo)
                            __instance.WriteSpawnMessage(innerNetObject, innerNetObject.OwnerId, innerNetObject.SpawnFlags, messageWriter);
                    }
                }
            }
            messageWriter.EndMessage();
            // log.Info($"send first data to {clientId}, size is {messageWriter.Length}", "SendInitialDataPrefix");
            __instance.SendOrDisconnect(messageWriter);
            messageWriter.Recycle();
        }
        DelaySpawnPlayerInfo(__instance, clientId);
        return false;
    }

    private static void DelaySpawnPlayerInfo(InnerNetClient __instance, int clientId)
    {
        List<NetworkedPlayerInfo> players = GameData.Instance.AllPlayers.ToArray().ToList();

        // We send 5 players at a time to prevent too huge packet
        while (players.Count > 0)
        {
            var batch = players.Take(5).ToList();

            MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
            messageWriter.StartMessage(6);
            messageWriter.Write(__instance.GameId);
            messageWriter.WritePacked(clientId);

            foreach (var player in batch)
            {
                if (messageWriter.Length > 1600) break;
                if (player != null && player.ClientId != clientId && !player.Disconnected)
                {
                    __instance.WriteSpawnMessage(player, player.OwnerId, player.SpawnFlags, messageWriter);
                }
                players.Remove(player);
            }
            messageWriter.EndMessage();
            // log.Info($"send delayed network data to {clientId} , size is {messageWriter.Length}", "SendInitialDataPrefix");
            __instance.SendOrDisconnect(messageWriter);
            messageWriter.Recycle();
        }
    }

    [QuickPrefix(typeof(InnerNetClient), nameof(InnerNetClient.SendAllStreamedObjects))]
    public static bool SendAllStreamedObjectsPrefix(InnerNetClient __instance, ref bool __result)
    {
        if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return true;
        // Bypass all NetworkedData here.
        __result = false;
        Il2CppSystem.Collections.Generic.List<InnerNetObject> obj = __instance.allObjects;
        lock (obj)
        {
            for (int i = 0; i < __instance.allObjects.Count; i++)
            {
                InnerNetObject innerNetObject = __instance.allObjects[i];
                if (innerNetObject && innerNetObject is not NetworkedPlayerInfo && innerNetObject.IsDirty && (innerNetObject.AmOwner || (innerNetObject.OwnerId == -2 && __instance.AmHost)))
                {
                    MessageWriter messageWriter = __instance.Streams[(int)innerNetObject.sendMode];
                    messageWriter.StartMessage(1);
                    messageWriter.WritePacked(innerNetObject.NetId);
                    try
                    {
                        if (innerNetObject.Serialize(messageWriter, false))
                        {
                            messageWriter.EndMessage();
                        }
                        else
                        {
                            messageWriter.CancelMessage();
                        }
                        if (innerNetObject.Chunked && innerNetObject.IsDirty)
                        {
                            __result = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Exception(ex);
                        messageWriter.CancelMessage();
                    }
                }
            }
        }
        for (int j = 0; j < __instance.Streams.Length; j++)
        {
            MessageWriter messageWriter2 = __instance.Streams[j];
            if (messageWriter2.HasBytes(7))
            {
                messageWriter2.EndMessage();
                __instance.SendOrDisconnect(messageWriter2);
                messageWriter2.Clear((SendOption)j);
                messageWriter2.StartMessage(5);
                messageWriter2.Write(__instance.GameId);
            }
        }
        return false;
    }

    [QuickPostfix(typeof(InnerNetClient), nameof(InnerNetClient.Spawn))]
    public static void SpawnPostfix(InnerNetClient __instance, InnerNetObject netObjParent, int ownerId = -2, SpawnFlags flags = SpawnFlags.None)
    {
        if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return;

        if (__instance.AmHost)
        {
            if (netObjParent is NetworkedPlayerInfo playerinfo)
            {
                Async.Schedule(() =>
                {
                    if (playerinfo != null && AmongUsClient.Instance.AmConnected)
                    {
                        var client = AmongUsClient.Instance.GetClient(playerinfo.ClientId);
                        if (client != null && !IsDisconnected(client))
                        {
                            if (playerinfo.IsIncomplete)
                            {
                                log.Info($"Disconnecting Client [{client.Id}]{client.PlayerName} {client.FriendCode} for playerinfo timeout");
                                AmongUsClient.Instance.SendLateRejection(client.Id, DisconnectReasons.ClientTimeout);
                                __instance.OnPlayerLeft(client, DisconnectReasons.ClientTimeout);
                            }
                        }
                    }
                }, 5f);
            }

            if (netObjParent is PlayerControl player)
            {
                Async.Schedule(() =>
                {
                    if (player != null && !player.notRealPlayer && !player.isDummy && AmongUsClient.Instance.AmConnected)
                    {
                        var client = AmongUsClient.Instance.GetClient(player.OwnerId);
                        if (client != null && !IsDisconnected(client))
                        {
                            if (player.Data == null || player.Data.IsIncomplete)
                            {
                                log.Info($"Disconnecting Client [{client.Id}]{client.PlayerName} {client.FriendCode} for playercontrol timeout");
                                AmongUsClient.Instance.SendLateRejection(client.Id, DisconnectReasons.ClientTimeout);
                                __instance.OnPlayerLeft(client, DisconnectReasons.ClientTimeout);
                            }
                        }
                    }
                }, 5.5f);
            }
        }

        if (!__instance.AmHost)
        {
            log.Exception("Tried to spawn while not host:" + (netObjParent?.ToString()));
        }
        return;
    }


    private static byte timer = 0;

    [QuickPostfix(typeof(InnerNetClient), nameof(InnerNetClient.FixedUpdate))]
    public static void FixedUpdatePostfix(InnerNetClient __instance)
    {
        // Send a networked data pre 2 fixed update should be a good practice?
        if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return;
        if (!__instance.AmHost || __instance.Streams == null) return;

        if (timer == 0)
        {
            timer = 1;
            return;
        }

        var player = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(x => x.IsDirty);
        if (player != null)
        {
            timer = 0;
            MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
            messageWriter.StartMessage(5);
            messageWriter.Write(__instance.GameId);
            messageWriter.StartMessage(1);
            messageWriter.WritePacked(player.NetId);
            try
            {
                if (player.Serialize(messageWriter, false))
                {
                    messageWriter.EndMessage();
                }
                else
                {
                    messageWriter.Recycle();
                    player.ClearDirtyBits();
                    return;
                }
                messageWriter.EndMessage();
                __instance.SendOrDisconnect(messageWriter);
                messageWriter.Recycle();
            }
            catch (Exception ex)
            {
                log.Exception(ex);
                messageWriter.CancelMessage();
                player.ClearDirtyBits();
            }
        }
    }

    // [QuickPrefix(typeof(InnerNetClient), nameof(InnerNetClient.SendOrDisconnect))]
    // public static void SendOrDisconnectPatch(InnerNetClient __instance, MessageWriter msg)
    // {
    //     if (DebugModeManager.IsDebugMode)
    //     {
    //         log.Info($"Packet({msg.Length}), SendOption:{msg.SendOption}", "SendOrDisconnectPatch");
    //     }
    //     else if (msg.Length > 1000)
    //     {
    //         log.Info($"Large Packet({msg.Length})", "SendOrDisconnectPatch");
    //     }
    // }
    private static bool IsDisconnected(ClientData client)
    {
        var __instance = AmongUsClient.Instance;
        for (int i = 0; i < __instance.allClients.Count; i++)
        {
            ClientData clientData = __instance.allClients[i];
            if (clientData.Id == client.Id)
            {
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.DirtyAllData))]
internal class DirtyAllDataPatch
{
    // Currently this function only occurs in CreatePlayer
    // It's believed to lag host, delay the playercontrol spawn mesasge & blackout new client
    // & send huge packets to all clients & completely no need to run
    // Temporarily disable it until Innersloth get a better fix.
    public static bool Prefix()
    {
        return false;
    }
}