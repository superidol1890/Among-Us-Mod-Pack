using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using InnerNet;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Network;
using TMPro;
using UnityEngine;
using VentLib;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

// Code from: https://github.com/Gurge44/EndlessHostRoles/blob/main/Modules/CustomNetObject.cs
// It is sort-of modified however.

// Sidenote:
// I HATE working with CustomRpcSender.
// It's just so weird.
// And it sucks because this is not a TOH-based mod so I have to convert everything over.

namespace Lotus.RPC.CustomObjects;

public class CustomNetObject
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(CustomNetObject));
    private static int MaxId = -1;

    public static readonly List<CustomNetObject> AllObjects = [];

    public PlayerControl playerControl;
    public Vector2 Position;
    public string Sprite;

    protected int Id = -1;

    private float playerControlTimer;

    private readonly HashSet<byte> HiddenList = [];

    public virtual bool CanTarget() => false;

    public virtual void SetupOutfit()
    {
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14><br></size>" + Sprite;
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = 255;
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = "";
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = "";
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
    }

    public void RpcChangeSprite(string sprite)
    {
        log.Info($" Change Custom Net Object {GetType().Name} (ID {Id}) sprite");

        Sprite = sprite;

        Async.Execute(() =>
        {
            playerControl.RawSetName(sprite);
            string name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
            int colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
            string hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
            string skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
            string petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
            string visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable); // Create Code

            // Start Message
            writer.StartMessage(5);
            writer.Write(AmongUsClient.Instance.GameId);

            SetupOutfit();
            writer.StartMessage(1);
            {
                writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                PlayerControl.LocalPlayer.Data.Serialize(writer, false);
            }

            writer.EndMessage();

            // Start RPC
            writer.StartMessage(2);
            writer.WritePacked(playerControl.NetId);
            writer.Write((byte)RpcCalls.Shapeshift);

            writer.WriteNetObject(PlayerControl.LocalPlayer);
            writer.Write(false);

            writer.EndMessage();

            ReverseEngineeredRPC.RpcChangeSkin(playerControl, PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default], sendToClients: false);
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
            writer.StartMessage(1);

            {
                writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                PlayerControl.LocalPlayer.Data.Serialize(writer, false);
            }

            writer.EndMessage();

            // 2nd endmessage
            writer.EndMessage();

            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        });
    }

    public void SnapTo(Vector2 position)
    {
        Position = position;
    }

    public void Despawn()
    {
        log.Info($" Despawn Custom Net Object {GetType().Name} (ID {Id})");

        try
        {
            playerControl.Despawn();
            AllObjects.Remove(this);
        }
        catch (Exception e)
        {
            log.Exception(e);
        }
    }

    protected void Hide(PlayerControl player)
    {
        log.Info($" Hide Custom Net Object {GetType().Name} (ID {Id}) from {player.GetNameWithRole()}");

        HiddenList.Add(player.PlayerId);

        if (player.AmOwner)
        {
            Async.Schedule(() => playerControl.transform.FindChild("Names").FindChild("NameText_TMP").gameObject.SetActive(false), 0.1f);
            playerControl.Visible = false;
            return;
        }

        if (playerControl.IsModded())
            Async.Schedule(() =>
            {
                // Vents.FindRPC((uint)ModCalls.FixModdedClientCNO).Send([player.GetClientId()], playerControl, false);
            }, 0.4f);

        MessageWriter writer = MessageWriter.Get();
        writer.StartMessage(6);
        writer.Write(AmongUsClient.Instance.GameId);
        writer.WritePacked(player.GetClientId());
        writer.StartMessage(5);
        writer.WritePacked(playerControl.NetId);
        writer.EndMessage();
        writer.EndMessage();
        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }

    protected virtual void OnFixedUpdate()
    {
        // need to respawn every 20 seconds because server will disconnect everyone on the 30th second.
        playerControlTimer += Time.fixedDeltaTime;
        if (playerControlTimer > 20f)
        {
            playerControlTimer = 0f;
            Async.Execute(RecreateNetObject);
        }

        CustomNetworkTransform nt = playerControl.NetTransform;
        if (nt == null) return;

        playerControl.Collider.enabled = false;

        if (Position != nt.body.position)
        {
            Transform transform = nt.transform;
            nt.body.position = Position;
            transform.position = Position;
            nt.body.velocity = Vector2.zero;
            nt.lastSequenceId++;
        }

        if (nt.HasMoved())
        {
            nt.sendQueue.Enqueue(nt.body.position);
            nt.SetDirtyBit(2U);
        }
    }

    protected void CreateNetObject(string sprite, Vector2 position)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        log.Info($"Create Custom Net Object {GetType().Name} (ID {MaxId + 1}) at {position}");
        playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
        playerControl.PlayerId = 254;
        playerControl.isNew = false;
        playerControl.notRealPlayer = true;
        AmongUsClient.Instance.NetIdCnt += 1U;

        MessageWriter msg = MessageWriter.Get();
        msg.StartMessage(5);
        msg.Write(AmongUsClient.Instance.GameId);
        AmongUsClient.Instance.WriteSpawnMessage(playerControl, -2, SpawnFlags.None, msg);
        msg.EndMessage();
        AmongUsClient.Instance.SendOrDisconnect(msg);
        msg.Recycle();

        if (ConnectionManager.IsVanillaServer)
        {
            MessageWriter msg2 = MessageWriter.Get(SendOption.Reliable);
            msg2.StartMessage(6);
            msg2.Write(AmongUsClient.Instance.GameId);
            msg2.WritePacked(int.MaxValue);
            for (uint i = 1; i <= 3; ++i)
            {
                msg2.StartMessage(4);
                msg2.WritePacked(2U);
                msg2.WritePacked(-2);
                msg2.Write((byte)SpawnFlags.None);
                msg2.WritePacked(1);
                msg2.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
                msg2.StartMessage(1);
                msg2.EndMessage();
                msg2.EndMessage();
            }
            msg2.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(msg2);
            msg2.Recycle();
        }

        if (PlayerControl.AllPlayerControls.Contains(playerControl)) PlayerControl.AllPlayerControls.Remove(playerControl);

        Async.Schedule(() =>
        {
            playerControl.NetTransform.RpcSnapTo(position);
            playerControl.RawSetName(sprite);
            string name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
            int colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
            string hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
            string skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
            string petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
            string visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable); // Create Code

            // Start Message
            writer.StartMessage(5);
            writer.Write(AmongUsClient.Instance.GameId);

            SetupOutfit();
            writer.StartMessage(1);

            {
                writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                PlayerControl.LocalPlayer.Data.Serialize(writer, false);
            }

            writer.EndMessage();

            writer.StartMessage(2);
            writer.WritePacked(playerControl.NetId);
            writer.Write((byte)RpcCalls.Shapeshift);

            writer.WriteNetObject(PlayerControl.LocalPlayer);
            writer.Write(false);

            writer.EndMessage();

            ReverseEngineeredRPC.RpcChangeSkin(playerControl, PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default], sendToClients: false);
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
            PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
            writer.StartMessage(1);

            {
                writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                PlayerControl.LocalPlayer.Data.Serialize(writer, false);
            }

            writer.EndMessage();
            // 2nd endmessage
            writer.EndMessage();

            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }, 0.2f);

        Position = position;
        Sprite = sprite;

        if (Id == -1)
        {
            if (MaxId != int.MaxValue) ++MaxId;
            Id = MaxId;

            AllObjects.Add(this);
        }

        foreach (PlayerControl pc in Players.GetAllPlayers())
        {
            if (pc.AmOwner) continue;

            Async.Schedule(() =>
            {
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable); // Create Code

                // StartMessage
                writer.StartMessage(6);
                writer.Write(AmongUsClient.Instance.GameId);
                writer.WritePacked(pc.GetClientId());

                writer.StartMessage(1);

                {
                    writer.WritePacked(playerControl.NetId);
                    writer.Write(pc.PlayerId);
                }

                writer.EndMessage();

                writer.StartMessage(2);
                writer.WritePacked(playerControl.NetId);
                writer.Write((byte)RpcCalls.MurderPlayer);

                writer.WriteNetObject(playerControl);
                writer.Write((int)MurderResultFlags.FailedError);

                writer.EndMessage();

                writer.StartMessage(1);

                {
                    writer.WritePacked(playerControl.NetId);
                    writer.Write((byte)254);
                }

                writer.EndMessage();
                writer.EndMessage();

                AmongUsClient.Instance.SendOrDisconnect(writer);
                writer.Recycle();
            }, 0.1f);
        }

        Async.Schedule(() => playerControl.transform.FindChild("Names").FindChild("NameText_TMP").gameObject.SetActive(true), 0.1f); // Fix for Host
        // Async.Schedule(() => Vents.FindRPC((uint)ModCalls.FixModdedClientCNO).Send([player.GetClientId()], playerControl), 0.4f); // Fix for Non-Host Modded
        Async.Schedule(() => RpcChangeSprite(sprite), .6f);
    }

    private void RecreateNetObject()
    {
        PlayerControl oldPlayerControl = playerControl;
        log.Trace("Recreating old net object.");
        Async.Schedule(() => oldPlayerControl.Despawn(), .3f);
        CreateNetObject(Sprite, Position);
    }

    public static void FixedUpdate()
    {
        foreach (CustomNetObject cno in AllObjects) cno?.OnFixedUpdate();
    }

    public static Optional<CustomNetObject> Get(int id)
    {
        return AllObjects.FirstOrOptional(x => x.Id == id);
    }

    public static Optional<CustomNetObject> ObjectFromPlayer(PlayerControl control) => AllObjects.FirstOrOptional(x => x.playerControl == control);

    public static void Reset()
    {
        try
        {
            AllObjects.ToArray().Do(x => x.Despawn());
            AllObjects.Clear();
        }
        catch (Exception e)
        {
            log.Exception(e);
        }
    }
}