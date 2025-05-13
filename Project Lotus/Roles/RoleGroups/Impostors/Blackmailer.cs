using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Chat;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using Lotus.Logging;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Impostors.Blackmailer.Translations;
using Lotus.API.Player;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Blackmailer : Shapeshifter
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Blackmailer));
    private Remote<TextComponent>? blackmailingText;
    private Optional<PlayerControl> blackmailedPlayer = Optional<PlayerControl>.Null();

    private bool showBlackmailedToAll;

    private int warnsUntilKick;
    private int currentWarnings;

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(LotusActionType.Shapeshift)]
    public void Blackmail(PlayerControl target, ActionHandle handle)
    {
        handle.Cancel();
        blackmailingText?.Delete();
        blackmailedPlayer = Optional<PlayerControl>.NonNull(target);
        TextComponent textComponent = new(new LiveString(BlackmailedText, Color.red), Game.InGameStates, viewers: MyPlayer);
        blackmailingText = target.NameModel().GetComponentHolder<TextHolder>().Add(textComponent);
    }

    [RoleAction(LotusActionType.RoundStart, ActionFlag.WorksAfterDeath)]
    public void ClearBlackmail()
    {
        blackmailedPlayer = Optional<PlayerControl>.Null();
        currentWarnings = 0;
        blackmailingText?.Delete();
    }

    [RoleAction(LotusActionType.RoundEnd, ActionFlag.WorksAfterDeath)]
    public void NotifyBlackmailed()
    {
        if (!blackmailedPlayer.Exists()) return;
        List<PlayerControl> allPlayers = showBlackmailedToAll
            ? Players.GetAllPlayers().ToList()
            : blackmailedPlayer.Transform(p => new List<PlayerControl> { p, MyPlayer }, () => new List<PlayerControl> { MyPlayer });
        if (!blackmailingText?.IsDeleted() ?? false) blackmailingText?.Get().SetViewerSupplier(() => allPlayers);
        blackmailedPlayer.IfPresent(p =>
        {
            string message = $"{RoleColor.Colorize(MyPlayer.name)} blackmailed {p.GetRoleColor().Colorize(p.name)}.";
            Game.MatchData.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, p, message));
            ChatHandler.Of(BlackmailedMessage, RoleColor.Colorize(RoleName)).Send(p);
        });
    }

    [RoleAction(LotusActionType.Exiled)]
    [RoleAction(LotusActionType.PlayerDeath)]
    private void BlackmailerDies()
    {
        blackmailedPlayer = Optional<PlayerControl>.Null();
        blackmailingText?.Delete();
    }

    [RoleAction(LotusActionType.Chat, ActionFlag.GlobalDetector | ActionFlag.WorksAfterDeath)]
    public void InterceptChat(PlayerControl speaker, GameState state, bool isAlive)
    {
        if (!isAlive || state is not GameState.InMeeting) return;
        if (!blackmailedPlayer.Exists() || speaker.PlayerId != blackmailedPlayer.Get().PlayerId) return;
        if (currentWarnings++ < warnsUntilKick)
        {
            ChatHandler.Of(WarningMessage, RoleColor.Colorize(RoleName)).Send(speaker);
            return;
        }

        log.Trace($"Blackmailer Killing Player: {speaker.name}");
        MyPlayer.InteractWith(speaker, new UnblockedInteraction(new FatalIntent(), this));
    }

    [RoleAction(LotusActionType.Disconnect, ActionFlag.GlobalDetector)]
    private void CheckForDisconnect(PlayerControl disconnecter)
    {
        if (!blackmailedPlayer.Exists() || disconnecter.PlayerId != blackmailedPlayer.Get().PlayerId) return;
        ClearBlackmail();
    }

    public override void HandleDisconnect() => ClearBlackmail();

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Warnings Until Death", Translations.Options.WarningsUntilDeath)
                .AddIntRange(0, 5, 1)
                .BindInt(i => warnsUntilKick = i)
                .Build())
            .SubOption(sub => sub.KeyName("Show Blackmailed to All", Translations.Options.ShowBlackmailedToAll)
                .AddOnOffValues()
                .BindBool(b => showBlackmailedToAll = b)
                .Build());

    [Localized(nameof(Blackmailer))]
    public static class Translations
    {
        [Localized(nameof(BlackmailedMessage))]
        public static string BlackmailedMessage = "You have been blackmailed! Sending a chat message will kill you.";

        [Localized(nameof(WarningMessage))]
        public static string WarningMessage = "You are not allowed to speak! If you speak again you may be killed.";

        [Localized(nameof(BlackmailedText))]
        public static string BlackmailedText = "BLACKMAILED";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(WarningsUntilDeath))]
            public static string WarningsUntilDeath = "Warnings Until Death";

            [Localized(nameof(ShowBlackmailedToAll))]
            public static string ShowBlackmailedToAll = "Show Blackmailed to All";
        }
    }
}