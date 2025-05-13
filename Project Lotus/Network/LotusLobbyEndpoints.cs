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

    public string CreateEndpoint() => IsDebug ? "https://testing-lotus.eps.lol/lobbies" : "https://lobbies.lotusau.top/lobbies";
    public string UpdatePlayerStatusEndpoint() => IsDebug ? "https://testing-lotus.eps.lol/update-lobby" : "https://lobbies.lotusau.top/update-lobby";
    public string UpdateMapEndpoint() => UpdatePlayerStatusEndpoint();
    public Dictionary<string, string> AddCustomHeaders(LobbyUpdateType _) => Empty;
}