﻿using System.Collections.Generic;
using System.Linq;
using Hazel;
using Hazel.Udp;
using InnerNet;
using Lotus.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Network;

public class ConnectionManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ConnectionManager));

    private static readonly Dictionary<long, byte> IPAddressPlayerMapping = new();


    [QuickPrefix(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public static void BindClientToIP(AmongUsClient __instance, ClientData data)
    {
        UnityUdpClientConnection connection = __instance.connection;
        Async.Schedule(() =>
        {
            if (data.Character == null) log.Trace($"Unable to map connection to player \"{data.PlayerName}\".", "ClientBinding");
            else
            {
                IPAddressPlayerMapping[connection.EndPoint.Address.Address] = data.Character.PlayerId;
                log.Trace($"Successfully bound connection of \"{data.Character.name} (ID={data.Character.PlayerId})", "ClientBinding");
            }
        }, 5f);
    }

    private static void TrackConnectionStatistics(Connection connection)
    {
        DevLogger.Log($"Packets Sent: {connection.Statistics.packetsSent} | Unreliable Packets Sent: {connection.Statistics.unreliableMessagesSent}");
    }


    public static bool IsVanillaServer
    {
        get
        {
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return false;

            const string Domain = "among.us";

            // From Reactor.gg
            return ServerManager.Instance.CurrentRegion?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
                regionInfo.PingServer.EndsWith(Domain, System.StringComparison.Ordinal) &&
                regionInfo.Servers.All(serverInfo => serverInfo.Ip.EndsWith(Domain, System.StringComparison.Ordinal));
        }
    }
}