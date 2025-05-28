﻿using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Chat.Commands;

[Command("death", "mydeath", "md")]
public class DeathCommand : CommandTranslations, ICommandReceiver
{
    public void Receive(PlayerControl source, CommandContext context)
    {
        if (!Game.MatchData.FrozenPlayers.Any())
        {
            ChatHandlers.InvalidCmdUsage(NoPreviousGameText).Send(source);
            return;
        }
        if (source.IsAlive() && Game.State is not GameState.InLobby)
        {
            ChatHandlers.InvalidCmdUsage().LeftAlign().Message(Translations.CannotViewDeathWhileAlive).Send(source);
            return;
        }
        if (context.Args.Length == 0) ShowMyDeath(source, Game.MatchData.FrozenPlayers[source.GetGameID()]);
        else
        {
            if (int.TryParse(context.Join(), out int id)) ShowDeathById(source, id);
            else if (context.Join("").ToLower().Contains(Translations.ListExtension)) ShowAllDeadPlayers(source);
            else ShowOtherPlayerDeath(source, context.Join());
        }
    }

    public static void ShowMyDeath(PlayerControl source, FrozenPlayer frozenPlayer)
    {
        if (frozenPlayer.Status == Managers.History.PlayerStatus.Alive)
        {
            CHandler(Translations.PlayerIsAliveText.Formatted(frozenPlayer.Name)).Send(UnityOptional<PlayerControl>.Of(source));
            return;
        }
        IDeathEvent? deathEvent = frozenPlayer.CauseOfDeath;
        if (deathEvent == null)
        {
            CHandler(Translations.CouldNotDetermineDeathText.Formatted(frozenPlayer.Name)).Send(UnityOptional<PlayerControl>.Of(source));
            return;
        }

        string death = deathEvent.InstigatorRole()
            .Transform(r => Translations.DirectDeathString.Formatted(frozenPlayer.Name, $"{deathEvent.Instigator().Map(p => p.Name).OrElse("Unknown")} ({r.RoleName})", deathEvent.SimpleName()),
                () => Translations.IndirectDeathString.Formatted(frozenPlayer.Name, deathEvent.SimpleName()));
        CHandler(death).Send(UnityOptional<PlayerControl>.Of(source));
    }

    private static void ShowOtherPlayerDeath(PlayerControl source, string playerName)
    {
        Game.MatchData.FrozenPlayers.Values.FirstOrOptional(fp => fp.Name.ToLower().Contains(playerName.ToLower()))
            .Handle(fp => ShowMyDeath(source, fp), () => CHandler(PlayerNotFoundText.Formatted(playerName)).Send(source));
    }

    private static void ShowDeathById(PlayerControl source, int id)
    {
        Game.MatchData.FrozenPlayers.Values.FirstOrOptional(fp => fp.PlayerId == id)
            .Handle(fp => ShowMyDeath(source, fp), () => CHandler(PlayerNotFoundText.Formatted(id)).Send(source));
    }

    private static void ShowAllDeadPlayers(PlayerControl source)
    {
        string outputText = Translations.ListMessageStart;

        Players.GetPlayers().ForEach(p =>
        {
            string inputName = $"{p.name} ({p.PlayerId})";
            if (p.IsAlive())
            {
                outputText += Translations.PlayerIsAliveText.Formatted(inputName);
                goto Finish;
            }

            Optional<IDeathEvent> optionalEvent = Game.MatchData.GameHistory.GetCauseOfDeath(p.PlayerId);
            if (!optionalEvent.Exists())
            {
                outputText += Translations.CouldNotDetermineDeathText.Formatted(inputName);
                goto Finish;
            }
            IDeathEvent deathEvent = optionalEvent.Get();

            string death = deathEvent.InstigatorRole()
                .Transform(r => Translations.DirectDeathString.Formatted(inputName, $"{deathEvent.Instigator().Map(inst => inst.Name).OrElse("Unknown")} ({r.RoleName})", deathEvent.SimpleName()),
                    () => Translations.IndirectDeathString.Formatted(inputName, deathEvent.SimpleName()));
            outputText += death.Replace("\n", ". ");

        Finish:
            outputText += "\n";
        });

        CHandler(outputText.Trim()).Send(source);
    }

    private static ChatHandler CHandler(string message)
    {
        return ChatHandler.Of(message, ModConstants.Palette.UndeadBlue.Colorize(Translations.DeathInfoTitle)).LeftAlign();
    }

    [Localized("MyDeath")]
    private static class Translations
    {
        [Localized(nameof(CannotViewDeathWhileAlive))]
        public static string CannotViewDeathWhileAlive = "You cannot view deaths while alive!";

        [Localized(nameof(DirectDeathString))]
        public static string DirectDeathString = "{0} died to {1}\nCause of Death: {2}";

        [Localized(nameof(IndirectDeathString))]
        public static string IndirectDeathString = "{0}'s cause of death: {1}";

        [Localized(nameof(CouldNotDetermineDeathText))]
        public static string CouldNotDetermineDeathText = "The cause of death of {0} could not be determined.";

        [Localized(nameof(PlayerIsAliveText))]
        public static string PlayerIsAliveText = "{0} is still alive and does not have a cause of death.";

        [Localized(nameof(DeathInfoTitle))]
        public static string DeathInfoTitle = "Death Info";

        [Localized(nameof(ListExtension))]
        public static string ListExtension = "list";

        [Localized(nameof(ListMessageStart))]
        public static string ListMessageStart = "Below is a list of everyone and their death reasons:";
    }
}