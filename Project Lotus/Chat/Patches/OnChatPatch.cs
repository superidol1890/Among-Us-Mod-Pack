using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Managers;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Operations;
using Lotus.Options;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class OnChatPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(OnChatPatch));

    internal static List<byte> UtilsSentList = new();
    public static bool EatMessage;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static bool Prefix(ChatController __instance, PlayerControl sourcePlayer, string chatText, bool censor)
    {
        log.Log(LogLevel.All, $"{sourcePlayer.name} => {chatText.Replace("\\", "(backwards slash here)")}"); // fix "\" crash. cant believe this crashes the game.
        if (!AmongUsClient.Instance.AmHost || censor == false) return true;
        if (UtilsSentList.Contains(sourcePlayer.PlayerId))
        {
            UtilsSentList.RemoveAt(UtilsSentList.FindIndex(b => b == sourcePlayer.PlayerId));
            return true;
        }
        Hooks.PlayerHooks.PlayerMessageHook.Propagate(new PlayerMessageHookEvent(sourcePlayer, chatText));
        if (!UseWordList() || !PluginDataManager.ChatManager.HasBannedWord(chatText) || sourcePlayer.IsHost())
        {
            if (PluginDataManager.TemplateManager.CheckAndRunCommand(sourcePlayer, chatText)) return true;
            bool eat = EatMessage;
            EatMessage = false;
            if (Game.State is GameState.InLobby) return !eat;
            ActionHandle handle = ActionHandle.NoInit();
            RoleOperations.Current.TriggerForAll(LotusActionType.Chat, sourcePlayer, handle, chatText, Game.State, sourcePlayer.IsAlive());
            return !eat;
        }
        AmongUsClient.Instance.KickPlayer(sourcePlayer.GetClientId(), false);
        ChatHandler.Send($"{sourcePlayer.name} was kicked by AutoKick.");
        return true;
    }

    public static bool UseWordList() => GeneralOptions.AdminOptions.AutoKick;
}