using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Options;
using Lotus.Extensions;
using Lotus.Utilities;
using VentLib;
using VentLib.Utilities.Extensions;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options;
using VentLib.Utilities.Collections;

namespace Lotus.RPC;

public static class GeneralModRpc
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(GeneralModRpc));

    [ModRPC((uint)ModCalls.Debug, RpcActors.Host, RpcActors.NonHosts)]
    public static void RpcDebug(string message)
    {
        log.Info($"(RpcDebug) Message from {Vents.GetLastSender((uint)ModCalls.Debug)!.name} => {message}");
        GameData.Instance.AllPlayers.ToArray().Select(p => (p.GetNameWithRole(), p.IsDead, p.IsIncomplete)).StrJoin().DebugLog("All Players: ");
    }

    [ModRPC((uint)ModCalls.SetKillCooldown, RpcActors.Host, RpcActors.NonHosts)]
    public static void RpcSetKillCooldown(float time)
    {
        PlayerControl.LocalPlayer.SetKillCooldown(time);
    }

    [ModRPC((uint)ModCalls.ShowChat, RpcActors.Host, RpcActors.NonHosts)]
    public static void ShowChat()
    {
        DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(true);
        DestroyableSingleton<HudManager>.Instance.Chat.HideBanButton();
    }

    [ModRPC((uint)ModCalls.SetGameState, RpcActors.Host, RpcActors.NonHosts)]
    public static void SetGameState(int state)
    {
        Game.State = (GameState)state;
    }
}