using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Subroles;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Crew.Medium.Translations;
using Object = UnityEngine.Object;

namespace Lotus.Roles.RoleGroups.Crew;
// IModdable
public class Medium : Crewmate
{
    public static HashSet<Type> MediumBannedModifiers = new() { typeof(Oblivious) };
    public override HashSet<Type> BannedModifiers() => MediumBannedModifiers;

    [NewOnSetup] private Dictionary<byte, Optional<CustomRole>> killerDictionary = new();
    private bool hasArrowsToBodies;
    private bool isClairvoyant;
    private int totalMeditates;
    private bool visibleToEveryone;

    private int mediatesLeft;
    private bool connectionEstablished;
    private byte reportedPlayer;

    [UIComponent(UI.Indicator)]
    private string Arrows() => hasArrowsToBodies ? Object.FindObjectsOfType<DeadBody>()
        .Where(b => !Game.MatchData.UnreportableBodies.Contains(b.ParentId))
        .Select(b => RoleUtils.CalculateArrow(MyPlayer, b, RoleColor)).Fuse("") : "";


    [RoleAction(LotusActionType.RoundStart)]
    private void ClearReportedPlayer()
    {
        reportedPlayer = byte.MaxValue;
        connectionEstablished = false;
    }

    [RoleAction(LotusActionType.PlayerDeath, ActionFlag.GlobalDetector)]
    private void AnyPlayerDeath(PlayerControl player, IDeathEvent deathEvent)
    {
        killerDictionary[player.PlayerId] = deathEvent.Instigator().Map(p => Optional<CustomRole>.Of(p.MyPlayer.GetCustomRoleSafe()).OrElse(p.MainRole));
    }

    [RoleAction(LotusActionType.ReportBody)]
    private void MediumDetermineRole(Optional<NetworkedPlayerInfo> reported)
    {
        if (!reported.Exists()) return;
        byte targetId = reported.Get().PlayerId;
        killerDictionary.GetOptional(targetId).FlatMap(o => o)
            .IfPresent(killerRole => Async.Schedule(() => MediumSendMessage(killerRole), 2f));
        reportedPlayer = targetId;
        EstablishMediumConnection();
    }


    [RoleAction(LotusActionType.Chat, ActionFlag.GlobalDetector)]
    private void RelayMessagesToMedium(PlayerControl chatter, string message)
    {
        if (!connectionEstablished || chatter.PlayerId != reportedPlayer) return;
        message = message.ToLower().Trim();
        if (string.Equals(message, YesAnswer) || string.Equals(message, NoAnswer))
        {
            mediatesLeft--;
            if (visibleToEveryone) Players.GetAlivePlayers().ForEach(ap => ChatHandler.Of(message, chatter.name).Send(ap));
            else ChatHandler.Of(message, chatter.name).Send(MyPlayer);

            if (mediatesLeft == 0) BreakMediumConnection(chatter);
        }
    }

    private void EstablishMediumConnection()
    {
        PlayerControl? deadPlayer = Players.FindPlayerById(reportedPlayer);
        if (deadPlayer == null || !isClairvoyant) return;
        MediumSendMessage(MediumConnection.Formatted(deadPlayer.name, totalMeditates == -1 ? ModConstants.Infinity : totalMeditates)).Send(MyPlayer);
        MediumSendMessage(MediumConnectionGhost.Formatted(MyPlayer.name)).Send(deadPlayer);
        connectionEstablished = true;
        mediatesLeft = totalMeditates;
    }

    private void MediumSendMessage(CustomRole killerRole)
    {
        ChatHandler.Of(MediumMessage.Formatted(killerRole.RoleColor.Colorize(killerRole.RoleName)))
            .Title(t => t.Prefix(".°").Suffix("°.").Color(RoleColor).Text(MediumTitle).Build())
            .Send(MyPlayer);
    }

    private ChatHandler MediumSendMessage(string message)
    {
        return ChatHandler.Of(message)
            .Title(t => t.Prefix(".°").Suffix("°.").Color(RoleColor).Text(MediumTitle).Build());
    }

    private void BreakMediumConnection(PlayerControl deadPlayer)
    {
        if (!connectionEstablished) return;
        MediumSendMessage(OutOfMeditates).Send(MyPlayer);
        MediumSendMessage(OutOfMeditatesGhost).Send(deadPlayer);
        connectionEstablished = false;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Has Arrows to Bodies", Translations.Options.HasArrowsToBody)
                .AddBoolean(false)
                .BindBool(b => hasArrowsToBodies = b)
                .Build())
            .SubOption(sub => sub
                .KeyName("Can Speak To Dead", Translations.Options.IsClairvoyant)
                .AddBoolean()
                .BindBool(b => isClairvoyant = b)
                .SubOption(sub2 => sub2
                    .KeyName("Amount of Meditates", Translations.Options.AmountOfMeditates)
                    .Value(v => v.Value(-1).Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Build())
                    .AddIntRange(1, ModConstants.MaxPlayers, 1)
                    .BindInt(i => totalMeditates = i)
                    .Build())
                .SubOption(sub2 => sub2
                    .KeyName("Visible to Everyone", Translations.Options.ShownToEveryone)
                    .AddBoolean(false)
                    .BindBool(b => visibleToEveryone = b)
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor("#A680FF");

    [Localized(nameof(Medium))]
    public static class Translations
    {
        [Localized(nameof(MediumTitle))] public static string MediumTitle = "Meditation";
        [Localized(nameof(MediumMessage))] public static string MediumMessage = "You've reported a body, and after great discussion with its spirits. You've determined the killer's role was {0}.";
        [Localized(nameof(MediumConnection))] public static string MediumConnection = "You're now able to speak with the ghost of {0}. They can answer with \"yes\" or \"no\" to your questions. You have {1} meditates left.";
        [Localized(nameof(MediumConnectionGhost))] public static string MediumConnectionGhost = "The Medium {0} has connected with you. You may talk to them by answering ONLY \"yes\" or \"no\".";
        [Localized(nameof(YesAnswer))] public static string YesAnswer = "yes";
        [Localized(nameof(NoAnswer))] public static string NoAnswer = "no";
        [Localized(nameof(OutOfMeditates))] public static string OutOfMeditates = "You are out of meditates. Your connection to the ghost has closed.";
        [Localized(nameof(OutOfMeditatesGhost))] public static string OutOfMeditatesGhost = "Your connection to the Medium has closed. Your answers are no longer heard.";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(HasArrowsToBody))] public static string HasArrowsToBody = "Has Arrows to Bodies";
            [Localized(nameof(CanSeeChatOfReportedPlayer))] public static string CanSeeChatOfReportedPlayer = "Can Speak with Ghost of Reported";
            [Localized(nameof(IsClairvoyant))] public static string IsClairvoyant = "Can Speak To Dead";
            [Localized(nameof(ShownToEveryone))] public static string ShownToEveryone = "Responses are Shown to Everyone";
            [Localized(nameof(AmountOfMeditates))] public static string AmountOfMeditates = "Amount of Meditates";
        }
    }
}

