﻿using System.Collections;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using BepInEx.Unity.IL2CPP.Utils;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Networking;

/// <summary>
/// Custom murder RPCs to fix issues with default ones.
/// </summary>
public static class CustomMurderRpc
{
    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    [MethodRpc((uint)MiraRpc.CustomMurder, LocalHandling = RpcLocalHandling.Before, SendImmediately = true)]
    public static void RpcCustomMurder(
        this PlayerControl source,
        PlayerControl target,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Custom Murder method without networking. If you need a networked version, use <see cref="RpcCustomMurder"/>.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="resultFlags">Murder result flags.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    public static void CustomMurder(
        this PlayerControl source,
        PlayerControl target,
        MurderResultFlags resultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        source.isKilling = false;
        var data = target.Data;
        if (resultFlags.HasFlag(MurderResultFlags.FailedError))
        {
            return;
        }

        if (resultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
            (resultFlags.HasFlag(MurderResultFlags.DecisionByHost) && target.protectedByGuardianId > -1))
        {
            target.protectedByGuardianThisRound = true;
            var flag = PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
            if (flag && PlayerControl.LocalPlayer.Data.PlayerId == target.protectedByGuardianId)
            {
                DataManager.Player.Stats.IncrementStat(StatID.Role_GuardianAngel_CrewmatesProtected);
                AchievementManager.Instance.OnProtectACrewmate();
            }

            if (source.AmOwner || flag)
            {
                target.ShowFailedMurder();

                if (resetKillTimer)
                {
                    source.SetKillTimer(
                        GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                }
            }
            else
            {
                target.RemoveProtection();
            }

            return;
        }

        if (!resultFlags.HasFlag(MurderResultFlags.Succeeded) &&
            !resultFlags.HasFlag(MurderResultFlags.DecisionByHost))
        {
            return;
        }

        DebugAnalytics.Instance.Analytics.Kill(target.Data, source.Data);
        if (source.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(
                GameManager.Instance.IsHideAndSeek()
                    ? StatID.HideAndSeek_ImpostorKills
                    : StatID.ImpostorKills);

            if (source.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
            {
                DataManager.Player.Stats.IncrementStat(StatID.Role_Shapeshifter_ShiftedKills);
            }

            if (Constants.ShouldPlaySfx() && playKillSound)
            {
                SoundManager.Instance.PlaySound(source.KillSfx, false, 0.8f);
            }

            if (resetKillTimer)
            {
                source.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            }
        }

        UnityTelemetry.Instance.WriteMurder();
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
        if (target.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(StatID.TimesMurdered);
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                    // ignored
                }
            }

            if (showKillAnim)
            {
                HudManager.Instance.KillOverlay.ShowKillAnimation(source.Data, data);
            }

            target.cosmetics.SetNameMask(false);
            target.RpcSetScanner(false);
        }

        AchievementManager.Instance.OnMurder(
            source.AmOwner,
            target.AmOwner,
            source.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
            source.shapeshiftTargetPlayerId,
            target.PlayerId);
        source.MyPhysics.StartCoroutine(source.KillAnimations.Random()?.CoPerformCustomKill(source, target, createDeadBody, teleportMurderer));
    }

    /// <summary>
    /// Perform a custom kill animation.
    /// </summary>
    /// <param name="anim">The kill animation.</param>
    /// <param name="source">The murderer.</param>
    /// <param name="target">The murdered player.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the murder be teleported.</param>
    /// <returns>Coroutine.</returns>
    public static IEnumerator CoPerformCustomKill(
        this KillAnimation anim,
        PlayerControl source,
        PlayerControl target,
        bool createDeadBody = true,
        bool teleportMurderer = true)
    {
        var cam = Camera.main?.GetComponent<FollowerCamera>();
        var isParticipant = PlayerControl.LocalPlayer == source || PlayerControl.LocalPlayer == target;
        var sourcePhys = source.MyPhysics;

        if (teleportMurderer)
        {
            KillAnimation.SetMovement(source, false);
        }

        KillAnimation.SetMovement(target, false);

        if (isParticipant)
        {
            PlayerControl.LocalPlayer.isKilling = true;
            source.isKilling = true;
        }

        DeadBody? deadBody = null;

        if (createDeadBody)
        {
            deadBody = Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
            deadBody.enabled = false;
            deadBody.ParentId = target.PlayerId;
            deadBody.bodyRenderers.ToList().ForEach(target.SetPlayerMaterialColors);

            target.SetPlayerMaterialColors(deadBody.bloodSplatter);
            var vector = target.transform.position + anim.BodyOffset;
            vector.z = vector.y / 1000f;
            deadBody.transform.position = vector;
        }

        if (isParticipant)
        {
            if (cam != null)
            {
                cam.Locked = true;
            }

            ConsoleJoystick.SetMode_Task();
            if (PlayerControl.LocalPlayer.AmOwner)
            {
                PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
            }
        }

        target.Die(DeathReason.Kill, true);

        if (teleportMurderer)
        {
            yield return source.MyPhysics.Animations.CoPlayCustomAnimation(anim.BlurAnim);
            sourcePhys.Animations.PlayIdleAnimation();
            source.NetTransform.SnapTo(target.transform.position);
            KillAnimation.SetMovement(source, true);
        }

        KillAnimation.SetMovement(target, true);

        if (deadBody != null)
        {
            deadBody.enabled = true;
        }

        var afterMurderEvent = new AfterMurderEvent(source, target, deadBody);
        MiraEventManager.InvokeEvent(afterMurderEvent);

        if (!isParticipant)
        {
            yield break;
        }

        if (cam != null)
        {
            cam.Locked = false;
        }

        PlayerControl.LocalPlayer.isKilling = false;
        source.isKilling = false;
    }
}
