using System.Collections.Generic;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Options;
using UnityEngine;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Chat.Patches;

static class ChatBubblePatch
{
    internal static readonly Queue<int> SetLeftQueue = new();

    [QuickPostfix(typeof(ChatBubble), nameof(ChatBubble.SetRight))]
    public static void SetBubbleRight(ChatBubble __instance)
    {
        if (SetLeftQueue.TryDequeue(out int _)) __instance.SetLeft();

        __instance.TextArea.richText = true;
    }

    [QuickPostfix(typeof(ChatBubble), nameof(ChatBubble.SetLeft))]
    public static void SetBubbleLeft(ChatBubble __instance)
    {
        SetLeftQueue.TryDequeue(out int _);
        __instance.TextArea.richText = true;
    }

    [QuickPostfix(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    public static void Postfix(ChatBubble __instance, bool isDead, bool voted)
    {
        PlayerControl seer = PlayerControl.LocalPlayer;
        PlayerControl target = __instance.playerInfo.Object;

        if (LobbyBehaviour.Instance == null)
        {
            if (!voted && seer.PlayerId == target.PlayerId) __instance.NameText.color = seer.GetRoleColor();
            else if (seer.PlayerId != target.PlayerId
                && (
                    seer.Relationship(target) is Relation.FullAllies
                    && seer.PrimaryRole().Faction.CanSeeRole(PlayerControl.LocalPlayer)
                ) | !seer.IsAlive()) __instance.NameText.color = target.GetRoleColor();
        }

        if (ClientOptions.VideoOptions.ChatDarkMode)
        {
            __instance.Background.color = new(0.1f, 0.1f, 0.1f, 1f);
            __instance.TextArea.color = Color.white;
            if (!__instance.playerInfo.Object.IsAlive() && LobbyBehaviour.Instance == null) __instance.Background.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        }
    }
}