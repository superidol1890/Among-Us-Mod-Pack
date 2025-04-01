using System.Linq;
using Lotus.Options;
using Lotus.Extensions;
using Lotus.Utilities;
using VentLib;
using VentLib.Utilities.Extensions;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options;
using VentLib.Utilities.Collections;

namespace Lotus.RPC;

public static class HostRpc
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(HostRpc));

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
}