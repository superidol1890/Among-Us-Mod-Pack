using LaunchpadReloaded.Features;
using LaunchpadReloaded.Options.Roles.Neutral;
using LaunchpadReloaded.Utilities;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Collections;
using LaunchpadReloaded.GameOver;
using LaunchpadReloaded.Roles.Outcast;
using MiraAPI.GameEnd;
using MiraAPI.Utilities;
using Helpers = MiraAPI.Utilities.Helpers;

namespace LaunchpadReloaded.Networking.Roles;
public static class ReaperRpc
{
    public static IEnumerator CoCollectEffects(DeadBody body)
    {
        body.Reported = true;
        body.enabled = false;

        var color = new UnityEngine.Color(LaunchpadPalette.ReaperColor.r, LaunchpadPalette.ReaperColor.g, LaunchpadPalette.ReaperColor.b, 0.6f);
        HudManager.Instance.FullScreen.gameObject.SetActive(true);
        HudManager.Instance.FullScreen.color = color;

        body.StartCoroutine(Effects.Shake(body.transform, 1, 0.01f, true, true));

        body.StartCoroutine(Effects.ColorFade(body.bodyRenderers[0], UnityEngine.Color.white, color, 1f));
        yield return HudManager.Instance.PlayerCam.CoShakeScreen(0.3f, 5f);
        HudManager.Instance.FullScreen.gameObject.SetActive(false);
    }

    [MethodRpc((uint)LaunchpadRpc.ReaperCollect)]
    public static void RpcCollectSoul(this PlayerControl playerControl, byte deadBody)
    {
        if (playerControl.Data.Role is not ReaperRole reaper)
        {
            playerControl.KickForCheating();
            return;
        }

        var body = Helpers.GetBodyById(deadBody);
        if (body == null) return;
        body.GetCacheComponent().isReaped = true;

        reaper.collectedSouls += 1;

        if (playerControl.AmOwner)
        {
            SoundManager.Instance.PlaySound(LaunchpadAssets.ReaperSound.LoadAsset(), false, 3f);
            Coroutines.Start(CoCollectEffects(body));
        }

        if (reaper.collectedSouls >= OptionGroupSingleton<ReaperOptions>.Instance.SoulCollections
            && PlayerControl.LocalPlayer.IsHost())
        {
            CustomGameOver.Trigger<ReaperGameOver>([reaper.Player.Data]);
        }
    }
}