using Hazel;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.GUI.Name.Interfaces;
using Lotus.Patches.Actions;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using VentLib;
using VentLib.Networking.RPC;
using VentLib.Networking.RPC.Interfaces;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using Object = UnityEngine.Object;

namespace Lotus.RPC;

public static class ReverseEngineeredRPC
{
    public static void RpcChangeSkin(PlayerControl player, NetworkedPlayerInfo.PlayerOutfit newOutift, int targetClientId = -1, bool sendToClients = true)
    {
        MassRpc massRpc = RpcV3.Mass();

        player.SetName(newOutift.PlayerName);
        if (sendToClients) massRpc.Start(player.NetId, RpcCalls.SetName).Write(player.Data.NetId).Write(newOutift.PlayerName).End();

        player.SetColor(newOutift.ColorId);
        if (sendToClients) massRpc.Start(player.NetId, RpcCalls.SetColor).Write(player.Data.NetId).Write((byte)newOutift.ColorId).End();

        player.SetHat(newOutift.HatId, newOutift.ColorId);
        if (sendToClients) massRpc.Start(player.NetId, RpcCalls.SetHatStr).Write(newOutift.HatId).Write(player.GetNextRpcSequenceId(RpcCalls.SetHatStr)).End();

        player.SetSkin(newOutift.SkinId, newOutift.ColorId);
        if (sendToClients) massRpc.Start(player.NetId, RpcCalls.SetSkinStr).Write(newOutift.SkinId).Write(player.GetNextRpcSequenceId(RpcCalls.SetSkinStr)).End();

        player.SetVisor(newOutift.VisorId, newOutift.ColorId);
        if (sendToClients) massRpc.Start(player.NetId, RpcCalls.SetVisorStr).Write(newOutift.VisorId).Write(player.GetNextRpcSequenceId(RpcCalls.SetVisorStr)).End();

        player.SetPet(newOutift.PetId);
        if (sendToClients) massRpc.Start(player.NetId, RpcCalls.SetPetStr).Write(newOutift.PetId).Write(player.GetNextRpcSequenceId(RpcCalls.SetPetStr)).End();

        player.SetNamePlate(newOutift.NamePlateId);
        if (sendToClients) massRpc.Start(player.NetId, RpcCalls.SetNamePlateStr).Write(newOutift.NamePlateId).Write(player.GetNextRpcSequenceId(RpcCalls.SetNamePlateStr)).End();

        if (sendToClients) massRpc.Send(targetClientId);
    }

    public static void EnableChatForPlayer(PlayerControl player)
    {
        if (player.AmOwner)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(true);
            DestroyableSingleton<HudManager>.Instance.Chat.HideBanButton();
            return;
        }
        if (player.IsModded())
        {
            Vents.FindRPC((uint)ModCalls.ShowChat)?.Send([player.OwnerId]);
            return;
        }
        bool isDead = player.Data.IsDead;
        MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
        messageWriter.StartMessage(6);
        messageWriter.Write(AmongUsClient.Instance.GameId);
        messageWriter.WritePacked(player.OwnerId);
        messageWriter.StartMessage(4);
        messageWriter.WritePacked(DestroyableSingleton<HudManager>.Instance.MeetingPrefab.SpawnId);
        messageWriter.WritePacked(-2);
        messageWriter.Write(0);
        messageWriter.WritePacked(1);
        uint netIdCnt = AmongUsClient.Instance.NetIdCnt;
        AmongUsClient.Instance.NetIdCnt = netIdCnt + 1U;
        messageWriter.WritePacked(netIdCnt);
        messageWriter.StartMessage(1);
        messageWriter.WritePacked(0);
        messageWriter.EndMessage();
        messageWriter.EndMessage();
        player.Data.IsDead = true;
        messageWriter.StartMessage(1);
        messageWriter.WritePacked(player.Data.NetId);
        player.Data.Serialize(messageWriter, true);
        messageWriter.EndMessage();
        messageWriter.StartMessage(2);
        messageWriter.WritePacked(netIdCnt);
        messageWriter.Write(22);
        messageWriter.EndMessage();
        player.Data.IsDead = isDead;
        messageWriter.StartMessage(1);
        messageWriter.WritePacked(player.Data.NetId);
        player.Data.Serialize(messageWriter, true);
        messageWriter.EndMessage();
        messageWriter.StartMessage(5);
        messageWriter.WritePacked(netIdCnt);
        messageWriter.EndMessage();
        messageWriter.EndMessage();
        AmongUsClient.Instance.SendOrDisconnect(messageWriter);
        messageWriter.Recycle();
    }

    public static IEnumerator UnshiftButtonTrigger(PlayerControl p)
    {
        // if (!p.PrimaryRole().RoleAbilityFlags.HasFlag(Roles.RoleAbilityFlag.UsesUnshiftTrigger)) yield break;
        PlayerControl target = Players.GetAlivePlayers().Where(t => t.PlayerId != p.PlayerId).ToList().GetRandom();
        p.RpcRejectShapeshift();
        // if (p.IsHost()) p.Shapeshift(target, false);
        // else RpcV3.Immediate(p.NetId, (byte)RpcCalls.Shapeshift).Write(target).Write(false).Send();

        // yield return new WaitForSeconds(NetUtils.DeriveDelay(0.05f));
        // RpcChangeSkin(p, p.Data.DefaultOutfit, p.GetClientId());

        // yield return new WaitForSeconds(NetUtils.DeriveDelay(0.05f));
        // INameModel nameModel = p.NameModel();
        // nameModel.Render(force: true);
        p.CRpcShapeshift(target, false);
        yield return new WaitForSeconds(NetUtils.DeriveDelay(0.1f));
        RpcChangeSkin(p, p.Data.DefaultOutfit, p.GetClientId());

        yield return new WaitForSeconds(NetUtils.DeriveDelay(0.1f));
        ShapeshiftFixPatch._shapeshifted.Remove(p.PlayerId);
        INameModel nameModel = p.NameModel();
        nameModel.Render(force: true);
    }
}