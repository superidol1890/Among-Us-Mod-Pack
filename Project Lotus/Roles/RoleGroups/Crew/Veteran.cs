using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Options;
using UnityEngine;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Crew;

public class Veteran : Crewmate
{
    [UIComponent(UI.Cooldown)]
    private Cooldown veteranCooldown = null!;
    private Cooldown veteranDuration = null!;

    private int totalAlerts;
    private int remainingAlerts;
    private bool canKillCrewmates;
    private bool canKillWhileTransported;
    private bool canKillRangedAttackers;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        remainingAlerts = totalAlerts;
    }

    [UIComponent(UI.Counter)]
    private string VeteranAlertCounter() => RoleUtils.Counter(remainingAlerts, totalAlerts);

    [UIComponent(UI.Indicator)]
    private string GetAlertedString() => veteranDuration.IsReady() ? "" : RoleColor.Colorize("♣");

    [RoleAction(LotusActionType.OnPet)]
    public void AssumeAlert()
    {
        if (remainingAlerts <= 0 || veteranCooldown.NotReady() || veteranDuration.NotReady()) return;
        VeteranAlertCounter().DebugLog("Veteran Alert Counter: ");
        veteranDuration.StartThenRun(() => veteranCooldown.Start());
        remainingAlerts--;
    }

    [RoleAction(LotusActionType.Interaction)]
    private void VeteranInteraction(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (veteranDuration.IsReady()) return;

        switch (interaction)
        {
            case Transporter.TransportInteraction when !canKillWhileTransported:
            case IRangedInteraction when !canKillRangedAttackers:
            case IDelayedInteraction:
            case IndirectInteraction:
                return;
        }

        if (actor.PrimaryRole().Faction.Relationship(this.Faction) is Relation.FullAllies && !canKillCrewmates) return;
        handle.Cancel();
        Game.MatchData.GameHistory.AddEvent(new VettedEvent(MyPlayer, actor));
        IDeathEvent deathEvent = new CustomDeathEvent(MyPlayer, actor, ModConstants.DeathNames.Parried);
        MyPlayer.InteractWith(actor, new LotusInteraction(new FatalIntent(interaction is not LotusInteraction, () => deathEvent), this));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Color(RoleColor)
            .SubOption(sub => sub
                .KeyName("Number of Alerts", Translations.Options.TotalAlerts)
                .Bind(v => totalAlerts = (int)v)
                .AddIntRange(1, 10, 1, 9).Build())
            .SubOption(sub => sub
                .KeyName("Alert Cooldown", Translations.Options.AlertCooldown)
                .Bind(v => veteranCooldown.Duration = (float)v)
                .AddFloatRange(2.5f, 120, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Alert Duration", Translations.Options.AlertDuration)
                .Bind(v => veteranDuration.Duration = (float)v)
                .AddFloatRange(1, 20, 0.25f, 10, GeneralOptionTranslations.SecondsSuffix).Build())
            .SubOption(sub => sub
                .KeyName("Kill Crewmates", Translations.Options.KillCrewmates)
                .Bind(v => canKillCrewmates = (bool)v)
                .AddOnOffValues().Build())
            .SubOption(sub => sub
                .KeyName("Kill While Transported", Translations.Options.KillWhileTransported)
                .Bind(v => canKillWhileTransported = (bool)v)
                .AddOnOffValues().Build())
            .SubOption(sub => sub
                .KeyName("Kill Ranged Attackers", Translations.Options.KillRangedAttackers)
                .BindBool(v => canKillRangedAttackers = v)
                .AddOnOffValues().Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Crewmate)
            .RoleColor(new Color(0.6f, 0.5f, 0.25f))
            .RoleAbilityFlags(RoleAbilityFlag.UsesPet);

    private class VettedEvent : KillEvent, IRoleEvent
    {
        public VettedEvent(PlayerControl killer, PlayerControl victim) : base(killer, victim)
        {
        }
    }

    [Localized(nameof(Veteran))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(TotalAlerts))]
            public static string TotalAlerts = "Number of Alerts";

            [Localized(nameof(AlertCooldown))]
            public static string AlertCooldown = "Alert Cooldown";

            [Localized(nameof(AlertDuration))]
            public static string AlertDuration = "Alert Duration";

            [Localized(nameof(KillCrewmates))]
            public static string KillCrewmates = "Kill Crewmates";

            [Localized(nameof(KillWhileTransported))]
            public static string KillWhileTransported = "Kill While Transported";

            [Localized(nameof(KillRangedAttackers))]
            public static string KillRangedAttackers = "Kill Ranged Attackers";
        }
    }
}