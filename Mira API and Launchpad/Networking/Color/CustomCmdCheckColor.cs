using System.Linq;
using Hazel;
using LaunchpadReloaded.Features.Managers;
using LaunchpadReloaded.Options;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace LaunchpadReloaded.Networking.Color;

[RegisterCustomRpc((uint)LaunchpadRpc.CustomCheckColor)]
public class CustomCmdCheckColor(LaunchpadReloadedPlugin plugin, uint id) : PlayerCustomRpc<LaunchpadReloadedPlugin, CustomColorData>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Write(MessageWriter writer, CustomColorData data)
    {
        writer.Write(data.ColorId);
        writer.Write(data.GradientId);
    }

    public override CustomColorData Read(MessageReader reader)
    {
        return new CustomColorData(reader.ReadByte(), reader.ReadByte());
    }

    public override void Handle(PlayerControl source, CustomColorData data)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var bodyColor = data.ColorId;
        var gradColor = data.GradientId;

        if (!OptionGroupSingleton<FunOptions>.Instance.UniqueColors.Value)
        {
            Rpc<CustomRpcSetColor>.Instance.Send(source, new CustomColorData(bodyColor, gradColor));
            return;
        }

        var allPlayers = GameData.Instance.AllPlayers.ToArray();

        var num = 0;
        while (num++ < 127 && (gradColor >= Palette.PlayerColors.Length || allPlayers.Any(p =>
                   !p.Disconnected && p.PlayerId != source.PlayerId && !VerifyColor(bodyColor, gradColor, p.PlayerId))))
        {
            gradColor = (byte)((gradColor + 1) % Palette.PlayerColors.Length);
        }

        num = 0;
        while (num++ < 127 && (bodyColor >= Palette.PlayerColors.Length || allPlayers.Any(p =>
                   !p.Disconnected && p.PlayerId != source.PlayerId && !VerifyColor(bodyColor, gradColor, p.PlayerId))))
        {
            bodyColor = (byte)((bodyColor + 1) % Palette.PlayerColors.Length);
        }

        Rpc<CustomRpcSetColor>.Instance.Send(source, new CustomColorData(bodyColor, gradColor));
    }

    private static bool VerifyColor(byte requestedColor, byte requestedGradient, byte playerId)
    {
        var data = GameData.Instance.GetPlayerById(playerId);

        if (!GradientManager.TryGetColor(playerId, out var gradColor))
        {
            Logger<LaunchpadReloadedPlugin>.Error($"Error getting gradient for player {data.PlayerName}");
            if (requestedColor == data.DefaultOutfit.ColorId)
            {
                return false;
            }
        }

        if (requestedColor == data.DefaultOutfit.ColorId && requestedGradient == gradColor)
        {
            return false;
        }

        if (requestedColor == gradColor && requestedGradient == data.DefaultOutfit.ColorId)
        {
            return false;
        }


        return true;
    }
}