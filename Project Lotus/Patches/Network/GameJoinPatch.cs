using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using Lotus.Addons;
using Lotus.API;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Options.General;
using VentLib.Utilities;

namespace Lotus.Patches.Network;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class GameJoinPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(GameJoinPatch));
    private static int _lastGameId;

    public static void Postfix(AmongUsClient __instance)
    {
        log.High($"Joining Lobby (GameID={__instance.GameId})", "GameJoin");
        SoundManager.Instance.ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);
        AddonManager.PlayerAddons = new();

        GameJoinHookEvent gameJoinHookEvent = new(_lastGameId != __instance.GameId || ServerAuthPatch.IsLocal);
        Hooks.NetworkHooks.GameJoinHook.Propagate(gameJoinHookEvent);
        _lastGameId = __instance.GameId;

        Async.WaitUntil(() => PlayerControl.LocalPlayer, p => p != null, p =>
        {
            gameJoinHookEvent.Loaded = true;
            AddonManager.SendAddonsToHost();
            PluginDataManager.TitleManager.ApplyTitleWithChatFix(p);
            if (AmongUsClient.Instance.AmHost) ModVersion.AddVersionShowerToPlayer(p, ModVersion.Version);
        }, 0.1f, 20);
        if (!AmongUsClient.Instance.AmHost) return;

        if (GeneralOptions.AdminOptions.AutoStartMaxTime != -1 && GeneralOptions.AdminOptions.AutoStartEnabled)
        {
            GeneralOptions.AdminOptions.AutoCooldown.SetDuration(GeneralOptions.AdminOptions.AutoStartMaxTime);
            GeneralOptions.AdminOptions.AutoCooldown.Start();
        }

        Async.Schedule(() =>
        {
            if (GeneralOptions.AdminOptions.AutoStartMaxTime != -1 && GeneralOptions.AdminOptions.AutoStartEnabled && GameStartManager.Instance.countDownTimer <= 0)
            {
                GameStartManager.Instance.BeginGame();
                float timeRemaining = GeneralOptions.AdminOptions.AutoCooldown.TimeRemaining();
                GameStartManager.Instance.countDownTimer = timeRemaining;
            }
            PlayerJoinPatch.CheckAutostart();
        }, 1f);
    }
}