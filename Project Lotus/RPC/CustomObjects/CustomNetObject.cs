using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using InnerNet;
using Lotus.API.Player;
using Lotus.Extensions;
using TMPro;
using UnityEngine;
using VentLib;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using Object = UnityEngine.Object;

// Credit: https://github.com/Gurge44/EndlessHostRoles/blob/main/Modules/CustomNetObject.cs
// Huge thanks to Gurge44 for letting me use his code!

// Sidenote:
// I HATE working with CustomRpcSender.
// It's just so weird.
// And it sucks because this is not a TOH-based mod so I have to convert everything over.

namespace Lotus.RPC.CustomObjects;

public class CustomNetObject
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(CustomNetObject));
    public static readonly List<CustomNetObject> AllObjects = [];
    private static int MaxId = -1;
    private readonly HashSet<byte> HiddenList = [];
    protected int Id;
    public PlayerControl playerControl;
    private float PlayerControlTimer;
    public Vector2 Position;

    public string Sprite;

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

        Async.Schedule(() =>
        {
            playerControl.RawSetName(sprite);
            string name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
            int colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
            string hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
            string skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
            string petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
            string visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
            MessageWriter writer = MessageWriter.Get(SendOption.None); // Create Code

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
        }, 0f);
    }

    public void SnapTo(Vector2 position)
    {
        playerControl.NetTransform.RpcSnapTo(position);
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
        PlayerControlTimer += Time.fixedDeltaTime;

        if (PlayerControlTimer > 20f)
        {
            log.Info($" Recreate Custom Net Object {GetType().Name} (ID {Id})");
            PlayerControl oldPlayerControl = playerControl;
            playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
            playerControl.PlayerId = 255;
            playerControl.isNew = false;
            playerControl.notRealPlayer = true;
            AmongUsClient.Instance.NetIdCnt += 1U;
            MessageWriter msg = MessageWriter.Get();
            msg.StartMessage(5);
            msg.Write(AmongUsClient.Instance.GameId);
            AmongUsClient.Instance.WriteSpawnMessage(playerControl, -2, SpawnFlags.None, msg);
            msg.EndMessage();
            msg.StartMessage(6);
            msg.Write(AmongUsClient.Instance.GameId);
            msg.WritePacked(int.MaxValue);

            for (uint i = 1; i <= 3; ++i)
            {
                msg.StartMessage(4);
                msg.WritePacked(2U);
                msg.WritePacked(-2);
                msg.Write((byte)SpawnFlags.None);
                msg.WritePacked(1);
                msg.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
                msg.StartMessage(1);
                msg.EndMessage();
                msg.EndMessage();
            }

            msg.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(msg);
            msg.Recycle();

            if (PlayerControl.AllPlayerControls.Contains(playerControl))
                PlayerControl.AllPlayerControls.Remove(playerControl);

            Async.Schedule(() =>
            {
                playerControl.NetTransform.RpcSnapTo(Position);
                playerControl.RawSetName(Sprite);
                string name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                int colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
                string hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
                string skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
                string petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
                string visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
                MessageWriter writer = MessageWriter.Get(SendOption.None); // Create Code

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
            }, 0.2f);

            Async.Schedule(() => oldPlayerControl.Despawn(), 0.3f);

            //playerControl.cosmetics.currentBodySprite.BodySprite.color = Color.clear;
            //playerControl.cosmetics.colorBlindText.color = Color.clear;
            foreach (PlayerControl pc in Players.GetAllPlayers())
            {
                if (pc.AmOwner) continue;

                Async.Schedule(() =>
                {
                    MessageWriter writer = MessageWriter.Get(SendOption.None); // Create Code

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
                        writer.Write((byte)255);
                    }

                    writer.EndMessage();
                    // 2nd endmessage
                    writer.EndMessage();

                    AmongUsClient.Instance.SendOrDisconnect(writer);
                    writer.Recycle();
                }, 0.1f);
            }

            foreach (PlayerControl pc in Players.GetAllPlayers())
                if (HiddenList.Contains(pc.PlayerId))
                    Hide(pc);

            Async.Schedule(() =>
            {
                // Fix for Host
                if (!HiddenList.Contains(PlayerControl.LocalPlayer.PlayerId))
                    playerControl.transform.FindChild("Names").FindChild("NameText_TMP").gameObject.SetActive(true);
            }, 0.1f);

            Async.Schedule(() =>
            {
                // Fix for Non-Host Modded
                foreach (PlayerControl visiblePC in Players.GetAllPlayers().ExceptBy(HiddenList, x => x.PlayerId))
                {
                    if (!visiblePC.IsModded()) continue;
                    // Vents.FindRPC((uint)ModCalls.FixModdedClientCNO).Send([player.GetClientId()], playerControl, true);
                }
            }, 0.4f);
            PlayerControlTimer = 0f;
        }
    }

    protected void CreateNetObject(string sprite, Vector2 position)
    {
        log.Info($" Create Custom Net Object {GetType().Name} (ID {MaxId + 1}) at {position}");
        playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
        playerControl.PlayerId = 255;
        playerControl.isNew = false;
        playerControl.notRealPlayer = true;
        AmongUsClient.Instance.NetIdCnt += 1U;
        MessageWriter msg = MessageWriter.Get();
        msg.StartMessage(5);
        msg.Write(AmongUsClient.Instance.GameId);
        AmongUsClient.Instance.WriteSpawnMessage(playerControl, -2, SpawnFlags.None, msg);
        msg.EndMessage();
        msg.StartMessage(6);
        msg.Write(AmongUsClient.Instance.GameId);
        msg.WritePacked(int.MaxValue);

        for (uint i = 1; i <= 3; ++i)
        {
            msg.StartMessage(4);
            msg.WritePacked(2U);
            msg.WritePacked(-2);
            msg.Write((byte)SpawnFlags.None);
            msg.WritePacked(1);
            msg.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
            msg.StartMessage(1);
            msg.EndMessage();
            msg.EndMessage();
        }

        msg.EndMessage();
        AmongUsClient.Instance.SendOrDisconnect(msg);
        msg.Recycle();
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
            MessageWriter writer = MessageWriter.Get(SendOption.None); // Create Code

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
        PlayerControlTimer = 0f;
        //playerControl.cosmetics.currentBodySprite.BodySprite.color = Color.clear;
        // playerControl.cosmetics.colorBlindText.color = Color.clear;
        Sprite = sprite;
        ++MaxId;
        Id = MaxId;
        if (MaxId == int.MaxValue) MaxId = int.MinValue;

        AllObjects.Add(this);

        foreach (PlayerControl pc in Players.GetAllPlayers())
        {
            if (pc.AmOwner) continue;

            Async.Schedule(() =>
            {
                MessageWriter writer = MessageWriter.Get(SendOption.None); // Create Code

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
                    writer.Write((byte)255);
                }

                writer.EndMessage();
                // 2nd endmessage
                writer.EndMessage();

                AmongUsClient.Instance.SendOrDisconnect(writer);
                writer.Recycle();
            }, 0.1f);
        }

        Async.Schedule(() => playerControl.transform.FindChild("Names").FindChild("NameText_TMP").gameObject.SetActive(true), 0.1f); // Fix for Host
        // Async.Schedule(() => Vents.FindRPC((uint)ModCalls.FixModdedClientCNO).Send([player.GetClientId()], playerControl), 0.4f); // Fix for Non-Host Modded
        // Async.Schedule(() => Utils.SendRPC(CustomRPC.FixModdedClientCNO, playerControl), 0.4f); // Fix for Non-Host Modded

    }

    public static void FixedUpdate()
    {
        foreach (CustomNetObject cno in AllObjects.ToArray()) cno?.OnFixedUpdate();
    }

    public static CustomNetObject Get(int id)
    {
        return AllObjects.FirstOrDefault(x => x.Id == id)!;
    }

    public static CustomNetObject ObjectFromPlayer(PlayerControl control) => AllObjects.FirstOrDefault(x => x.playerControl == control)!;

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