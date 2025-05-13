using Lotus.Extensions;
using Lotus.Managers;
using Lotus.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Chat.Commands;

[Localized("Commands.Admin")]
[Command("kick", "ban")]
public class KickBanCommand : ICommandReceiver
{
    [Localized("KickMessage")] private static string _kickedMessage = "{0} was kicked by host.";
    [Localized("BanMessage")] private static string _banMessage = "{0} was banned by host.";

    public void Receive(PlayerControl source, CommandContext context)
    {
        if (!PluginDataManager.ModManager.IsPlayerModded(source) && !source.IsHost())
        {
            ChatHandlers.NotPermitted().Send(source);
            return;
        }
        bool ban = context.Alias == "ban";
        string message = ban ? _banMessage : _kickedMessage;

        if (context.Args.Length == 0)
        {
            BasicCommands.PlayerIds(source, context);
            return;
        }

        Optional<PlayerControl> targetPlayer = Optional<PlayerControl>.Null();
        string text = context.Join();
        bool banWithId = false;
        if (int.TryParse(context.Args[0], out int result))
        {
            targetPlayer = Utils.PlayerById((byte)result);
            banWithId = true;
        }
        else targetPlayer = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrOptional(p => p.name.ToLowerInvariant().Equals(text.ToLowerInvariant()));

        targetPlayer.Handle(player =>
        {
            if (player.IsHost())
            {
                ChatHandlers.InvalidCmdUsage("Not able to ban the host from the game.").Send();
                return;
            }
            if (LobbyBehaviour.Instance == null) player.RpcExileV2(false);
            if (banWithId && context.Args.Length > 1)
            {
                string reason = context.Args[1..].Fuse(" ");
                if (reason.Trim() == string.Empty) reason = "Host Decision";
                PluginDataManager.BanManager.BanWithReason(player, reason, $"{player.name} was banned for {reason}.");
            }
            else
            {
                string reason = context.Args[1..].Fuse(" ");
                if (reason.Trim() == string.Empty) AmongUsClient.Instance.KickPlayer(player.GetClientId(), ban);
                else AmongUsClient.Instance.KickPlayerWithMessage(player, reason, false);
            }
            ChatHandler.Of(message.Formatted(player.name)).Send();
        }, () => ChatHandler.Of($"Unable to find player: {text}").Send(source));
    }
}