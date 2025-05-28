using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VentLib.Lobbies;

namespace Lotus.Network;

public class LotusLobbyEndpoints : ILobbyServerInfo
{
    private static readonly Dictionary<string, string> Empty = new();
    private const bool IsDebug = false;

    public string CreateEndpoint() => IsDebug ? $"https://testing-lotus.eps.lol/lobbies/{AmongUsClient.Instance.GameId}" : $"https://lobbies.lotusau.top/lobbies/{AmongUsClient.Instance.GameId}";
    public string UpdatePlayerStatusEndpoint() => IsDebug ? $"https://testing-lotus.eps.lol/lobbies/{AmongUsClient.Instance.GameId}/update" : $"https://lobbies.lotusau.top/lobbies/{AmongUsClient.Instance.GameId}/update";
    public string UpdateMapEndpoint() => UpdatePlayerStatusEndpoint();
    public Dictionary<string, string> AddCustomHeaders(LobbyUpdateType _) => Empty;
}