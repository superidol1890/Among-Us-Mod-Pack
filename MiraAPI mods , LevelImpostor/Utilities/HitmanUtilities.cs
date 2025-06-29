using AmongUs.Data;
using LaunchpadReloaded.Buttons.Impostor;
using LaunchpadReloaded.Features;
using LaunchpadReloaded.Roles.Impostor;
using MiraAPI.Hud;
using MiraAPI.Networking;
using Reactor.Utilities.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LaunchpadReloaded.Utilities;
public static class HitmanUtilities
{
    private static float LastMarkTime;
    private static float OgShakeAmount;
    private static float OgShakePeriod;
    private static float OgSpeed;

    private static bool OgShake;
    public static IEnumerator? ClockTick;
    public static IEnumerator? OverlayTick = null;
    public static List<PlayerControl>? MarkedPlayers;

    public static void Initialize()
    {
        ClockTick = null;
        MarkedPlayers = [];

        var followerCam = HudManager.Instance.PlayerCam;
        OgShakeAmount = followerCam.shakeAmount;
        OgShakePeriod = followerCam.shakePeriod;
        followerCam.shakeAmount = 0.15f;
        followerCam.shakePeriod = 1.2f;

        OgShake = DataManager.Settings.Gameplay.screenShake;

        OgSpeed = PlayerControl.LocalPlayer.MyPhysics.Speed;
        PlayerControl.LocalPlayer.MyPhysics.Speed = OgSpeed * 0.75f;

        LastMarkTime = Time.unscaledTime;
    }

    public static void Deinitialize()
    {
        var followerCam = HudManager.Instance!.PlayerCam;
        followerCam.shakeAmount = OgShakeAmount;
        followerCam.shakePeriod = OgShakePeriod;
        PlayerControl.LocalPlayer.MyPhysics.Speed = OgSpeed;
        DataManager.Settings.Gameplay.ScreenShake = OgShake;
    }

    public static void PlayerMarkCheck()
    {
        if (!Input.GetMouseButtonDown(0) || !(Time.unscaledTime - LastMarkTime >= 1)) return;
        var mousePos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        var plr = Helpers.GetPlayerToPoint(new Vector3(mousePos.x, mousePos.y, 0));

        if (MarkedPlayers == null || plr == null)
        {
            return;
        }

        if (MarkedPlayers.Contains(plr) || plr == PlayerControl.LocalPlayer)
        {
            return;
        }

        CreatePlayerMark(plr);
    }

    private static void CreatePlayerMark(PlayerControl plr)
    {
        LastMarkTime = Time.unscaledTime;
        MarkedPlayers?.Add(plr);

        GameObject mark = new("Mark");
        mark.transform.SetParent(plr.transform);
        mark.transform.localPosition = new Vector3(0f, 0f, -1f);
        mark.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        var rend = mark.gameObject.AddComponent<SpriteRenderer>();
        rend.sprite = LaunchpadAssets.DeadlockTarget.LoadAsset();
        SoundManager.Instance.PlaySound(LaunchpadAssets.DeadlockMark.LoadAsset(), false, 2f);

        HudManager.Instance.StartCoroutine(Effects.Bloop(0.1f, mark.transform, 0.25f, 1f));
        HudManager.Instance.StartCoroutine(HudManager.Instance.PlayerCam.CoShakeScreen(0.4f, 3f));
    }

    public static void ClearMarks()
    {
        if (MarkedPlayers is not { Count: > 0 })
        {
            return;
        }

        foreach (var plr in MarkedPlayers)
        {
            var mark = plr.transform.Find("Mark");
            if (!mark) continue;
            mark.gameObject.Destroy();
        }

        MarkedPlayers.Clear();
        MarkedPlayers = null;
    }

    public static IEnumerator KillMarkedPlayers()
    {
        var origCount = MarkedPlayers!.Count;

        while (MarkedPlayers.Count > 0)
        {
            var player = MarkedPlayers[0];
            MarkedPlayers.RemoveAt(0);

            if (player)
            {
                PlayerControl.LocalPlayer.RpcCustomMurder(player, resetKillTimer: false, createDeadBody: true,
                    teleportMurderer: false, showKillAnim: true);

                var mark = player.transform.Find("Mark");
                if (mark) mark.gameObject.Destroy();
            }

            yield return new WaitForSeconds(0.2f);
        }

        if (origCount > 0)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification($"  <size=130%>- (x{origCount})</size>", Color.red, new Vector3(0f, 1.4f, -2f),
                 LaunchpadAssets.DeadlockKillConfirmal.LoadAsset(), LaunchpadAssets.DeadlockHonor.LoadAsset());
        }
    }

    public static IEnumerator TransitionColor(Color targetColor, SpriteRenderer rend, float time = 1f, System.Action? action = null)
    {
        var startColor = rend.color;
        var elapsedTime = 0f;

        while (elapsedTime < time)
        {
            rend.color = Color.Lerp(startColor, targetColor, elapsedTime / time);
            elapsedTime += Time.deltaTime * 4f;
            yield return null;
        }

        rend.color = targetColor;
        action?.Invoke();
    }

    public static IEnumerator ClockTickRoutine()
    {
        var button = CustomButtonSingleton<DeadlockButton>.Instance;

        while (HudManager.InstanceExists && button.EffectActive)
        {
            var tickInterval = Mathf.Lerp(1.5f, 0.2f, (button.EffectDuration - button.Timer) / button.EffectDuration);
            SoundManager.Instance.StopSound(LaunchpadAssets.DeadlockClockLeft.LoadAsset());
            SoundManager.Instance.PlaySound(LaunchpadAssets.DeadlockClockLeft.LoadAsset(), false, 0.5f);
            yield return new WaitForSeconds(tickInterval);
            SoundManager.Instance.StopSound(LaunchpadAssets.DeadlockClockRight.LoadAsset());
            SoundManager.Instance.PlaySound(LaunchpadAssets.DeadlockClockRight.LoadAsset(), false, 0.5f);
            yield return new WaitForSeconds(tickInterval);
        }
    }

    public static IEnumerator OverlayColorRoutine()
    {
        var button = CustomButtonSingleton<DeadlockButton>.Instance;
        var role = PlayerControl.LocalPlayer.Data.Role as HitmanRole;
        if (role == null)
        {
            yield break;
        }

        while (HudManager.InstanceExists && button.EffectActive)
        {
            var progress = (button.EffectDuration - button.Timer) / button.EffectDuration;
            var newAlpha = Mathf.Lerp(0.4f, 1f, progress);

            var newColor = role.OverlayColor;
            newColor.a = newAlpha;
            if (role.OverlayRend != null)
            {
                role.OverlayRend.color = newColor;
            }

            yield return null;
        }
    }
}
