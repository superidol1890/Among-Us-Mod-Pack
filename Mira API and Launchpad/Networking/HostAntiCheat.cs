using LaunchpadReloaded.Options;
using MiraAPI.GameOptions;

namespace LaunchpadReloaded.Networking;

public static class HostAntiCheat
{
    public static void KickForCheating(this PlayerControl hacker)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var id = AmongUsClient.Instance.GetClientIdFromCharacter(hacker);
        AmongUsClient.Instance.KickPlayer(id, OptionGroupSingleton<GeneralOptions>.Instance.BanCheaters);
    }
}