using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Managers.History;
using Lotus.Managers.Hotkeys;
using Lotus.Utilities;
using Lotus.Victory.Conditions;
using Lotus.Options;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Debug.Profiling;

namespace Lotus.Victory.Patches;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public class CheckEndGamePatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(CheckEndGamePatch));

    public static bool BeginWin;
    public static bool Deferred;
    private static DateTime slowDown = DateTime.Now;
    private static FixedUpdateLock _fixedUpdateLock = new(0.1f);

    static CheckEndGamePatch()
    {
        HotkeyManager.Bind(KeyCode.LeftShift, KeyCode.L, KeyCode.Return)
            .If(p => p.HostOnly().State(Game.InGameStates))
            .Do(() =>
            {
                Deferred = false;
                ManualWin manualWin = new(new List<PlayerControl>(), ReasonType.HostForceEnd);
                manualWin.Activate();
                GameManager.Instance.LogicFlow.CheckEndCriteria();
                Hooks.ResultHooks.ForceEndGameHook.Propagate(new EmptyHookEvent());
            });
        Hooks.PlayerHooks.PlayerDisconnectHook.Bind(nameof(CheckEndGamePatch), _ => ForceCheckEndGame());
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool Prefix()
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (Game.State is GameState.InLobby or GameState.InIntro) return false;
        if (!_fixedUpdateLock.AcquireLock()) return false;
        if (Deferred) return false;

        uint id = Profilers.Global.Sampler.Start("CheckEndGamePatch");

        WinDelegate winDelegate = Game.GetWinDelegate();
        if (GeneralOptions.DebugOptions.NoGameEnd) winDelegate.CancelGameWin();

        bool isGameWin = winDelegate.IsGameOver();
        if (!isGameWin)
        {
            Profilers.Global.Sampler.Stop(id, "CheckEndGamePatch-NotGameWin");
            return false;
        }


        List<PlayerControl> winners = winDelegate.GetWinners().DistinctBy(p => p.PlayerId).ToList();
        if (winners == null!) winners = new List<PlayerControl>();
        bool impostorsWon = winners.Count == 0 || winners[0].Data.Role.IsImpostor;

        GameOverReason reason = winDelegate.GetWinReason().ReasonType switch
        {
            ReasonType.FactionLastStanding => impostorsWon ? GameOverReason.ImpostorsByKill : GameOverReason.CrewmatesByVote,
            ReasonType.RoleSpecificWin => impostorsWon ? GameOverReason.ImpostorsByKill : GameOverReason.CrewmatesByVote,
            ReasonType.TasksComplete => GameOverReason.CrewmatesByTask,
            ReasonType.Sabotage => GameOverReason.ImpostorsBySabotage,
            ReasonType.NoWinCondition => GameOverReason.ImpostorDisconnect,
            ReasonType.HostForceEnd => GameOverReason.ImpostorDisconnect,
            ReasonType.GameModeSpecificWin => GameOverReason.ImpostorsByKill,
            ReasonType.SoloWinner => GameOverReason.ImpostorsByKill,
        };


        Game.MatchData.GameHistory.PlayerHistory = Game.MatchData.FrozenPlayers.Values.Select(p => new PlayerHistory(p)).ToList();
        VictoryScreen.ShowWinners(winDelegate, reason);

        Deferred = true;
        BeginWin = true;
        Async.Schedule(() => DelayedWin(reason), NetUtils.DeriveDelay(0.01f));

        Profilers.Global.Sampler.Stop(id);
        return false;
    }

    private static void DelayedWin(GameOverReason reason)
    {
        Deferred = false;
        BeginWin = false;
        log.Info("Ending Game");
        GameManager.Instance.RpcEndGame(reason, false);
        Async.Schedule(() => GameManager.Instance.EndGame(), 0.1f);
    }

    public static bool ForceCheckEndGame()
    {
        if (BeginWin) return BeginWin;
        bool wasDeferred = Deferred;
        Deferred = false;
        _fixedUpdateLock.Unlock();
        Prefix();
        Deferred = wasDeferred;
        return BeginWin;
    }
}