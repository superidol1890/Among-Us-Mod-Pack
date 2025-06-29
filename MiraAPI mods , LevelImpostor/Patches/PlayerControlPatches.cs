using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities.Extensions;

namespace MiraAPI.Patches;

/// <summary>
/// General patches for the PlayerControl class.
/// </summary>
[HarmonyPatch(typeof(PlayerControl))]
internal static class PlayerControlPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.Start))]
    // ReSharper disable once InconsistentNaming
    public static void PlayerControlStartPostfix(PlayerControl __instance)
    {
        if (__instance.gameObject.TryGetComponent<ModifierComponent>(out var modifierComp))
        {
            modifierComp.DestroyImmediate();
        }

        if (__instance.gameObject.TryGetComponent<PlayerVoteData>(out var voteComp))
        {
            voteComp.DestroyImmediate();
        }

        __instance.gameObject.AddComponent<ModifierComponent>();
        __instance.gameObject.AddComponent<PlayerVoteData>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.Die))]
    // ReSharper disable once InconsistentNaming
    public static void PlayerControlDiePostfix(PlayerControl __instance, DeathReason reason)
    {
        var deathEvent = new PlayerDeathEvent(__instance, reason, Helpers.GetBodyById(__instance.PlayerId));
        MiraEventManager.InvokeEvent(deathEvent);

        var modifiersComponent = __instance.GetComponent<ModifierComponent>();

        if (modifiersComponent)
        {
            modifiersComponent.ActiveModifiers.ForEach(x => x.OnDeath(reason));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.CompleteTask))]
    // ReSharper disable once InconsistentNaming
    public static void PlayerCompleteTaskPostfix(PlayerControl __instance, uint idx)
    {
        var playerTask = __instance.myTasks.ToArray().First(playerTask => playerTask.Id == idx);
        if (playerTask != null)
        {
            var completeTaskEvent = new CompleteTaskEvent(__instance, playerTask);
            MiraEventManager.InvokeEvent(completeTaskEvent);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerControl.RpcMurderPlayer))]
    // ReSharper disable once InconsistentNaming
    // Note: This method is partially inlined in 2025.5.20 but we already use custom murder RPCs.
    public static void PlayerControlMurderPrefix(PlayerControl __instance, PlayerControl target, ref bool didSucceed)
    {
        var beforeMurderEvent = new BeforeMurderEvent(__instance, target);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        didSucceed = beforeMurderEvent.IsCancelled;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    // ReSharper disable once InconsistentNaming
    public static void PlayerControlFixedUpdatePostfix(PlayerControl __instance)
    {
        if (!__instance.AmOwner)
        {
            return;
        }

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            if (__instance.Data?.Role == null)
            {
                continue;
            }

            if (!button.Enabled(__instance.Data?.Role))
            {
                continue;
            }

            button.FixedUpdateHandler(__instance);
        }
    }
}
