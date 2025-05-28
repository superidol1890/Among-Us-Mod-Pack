using System.Collections.Generic;
using System.IO;
using System.Linq;
using InnerNet;
using Lotus.Extensions;
using Lotus.Options;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers;

[Localized("Whitelist")]
public class WhitelistManager
{
    [Localized("WhitelistBan")] private static string _whitelistBan = "{0} was kicked because they weren't on the whitelist.";

    private List<string> whitelistedFriendcodes = new();
    private FileInfo file;

    public WhitelistManager(FileInfo fileInfo)
    {
        this.file = fileInfo;
        StreamReader reader = new(file.Open(FileMode.OpenOrCreate));
        List<string> lines = reader.ReadToEnd().Split("\n").Where(l => l != "\n").Where(l => l != "").Select(f => f.Replace("\r", "")).Distinct().ToList();
        reader.Close();
        whitelistedFriendcodes.AddRange(lines);
    }

    public bool CheckWhitelist(ClientData player)
    {
        return !GeneralOptions.AdminOptions.EnableWhitelist
               || AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame
               || whitelistedFriendcodes.Contains(player.FriendCode);
    }

    public bool CheckWhitelistPlayer(PlayerControl player, ClientData client)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        if (CheckWhitelist(client)) return false;

        string message = _whitelistBan.Formatted(player.name);
        AmongUsClient.Instance.KickPlayerWithMessage(player, message);
        return true;
    }

    public List<string> Whitelisted() => whitelistedFriendcodes;
}