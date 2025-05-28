using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Factions.Crew;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Holders;
using Lotus.Managers.History.Events;
using Lotus.Options;
using Lotus.Roles.Factions;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals;
using Lotus.Roles.Managers.Interfaces;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Jackal : NeutralKillingBase
{
    public static readonly Color JackalColor = new(0f, 0.71f, 0.92f);

    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Jackal));
    private static readonly Sidekick Sidekick = new();
    private const bool SidekickGetsAbilities = false;

    public bool CanRecruit; // A setting but is also for setting if they can recruit for sidekick.

    private bool canVent;
    private int killsForRecruit;
    private bool impostorVision;
    private bool sidekickCanRecruit;
    private bool sidekickBecomesJackal;
    private NonCrewRecruit onNonCrewRecruit;

    private byte sidekickId = byte.MaxValue;
    private bool hasRecruited;
    private int currentKills;

    [UIComponent(UI.Counter)]
    private string KillCount() => CanRecruit ? RoleUtils.Counter(currentKills, killsForRecruit, JackalColor) : "";

    [UIComponent(UI.Text)]
    private string CanRecruitText() => CanRecruit && !hasRecruited && currentKills >= killsForRecruit ? JackalColor.Colorize(Translations.NextKillIsRecruit) : "";

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!CanRecruit) return base.TryKill(target);

        if (!hasRecruited)
        {
            if (killsForRecruit >= currentKills)
            {
                TryRecruit(target);
                return false;
            }
        }

        bool flag = base.TryKill(target);
        if (flag) currentKills++;
        return flag;
    }

    // Set sidekick as Jackal on Disconnect.
    public override void HandleDisconnect() => MyDeath();

    [RoleAction(LotusActionType.PlayerDeath)]
    private void MyDeath()
    {
        if (sidekickId == byte.MaxValue) return;
        if (!sidekickBecomesJackal) return;
        log.Debug($"Checking if sidekick still exists. (sidekickId: {sidekickId})");
        Utils.PlayerById(sidekickId).IfPresent(sidekickPlayer =>
        {
            if (!sidekickPlayer.IsAlive()) return; // Don't change role if player is dead.
            log.Debug("Found sidekick player.");
            sidekickPlayer.PrimaryRole().ChangeRoleTo(this);

            Jackal jackalRole = sidekickPlayer.PrimaryRole<Jackal>()!;
            jackalRole.CanRecruit = sidekickCanRecruit;
        });
    }

    public void OnSidekickDisconnect()
    {
        log.Debug("Sidekick disconnected.");
        sidekickId = byte.MaxValue;
    }

    private void TryRecruit(PlayerControl target)
    {
        log.Debug($"Trying to recruit {target.GetNameWithRole()}.");
        if (target.PrimaryRole().Faction is not Crewmates && onNonCrewRecruit is NonCrewRecruit.Kill)
        {
            bool flag = base.TryKill(target);
            if (flag) currentKills++;
            return;
        }

        InteractionResult result = MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this));
        if (result is InteractionResult.Halt) return; // Stop convert if it was canceled by role
        log.Debug("Successfully recruited.");

        hasRecruited = true;
        sidekickId = target.PlayerId;

        target.PrimaryRole().ChangeRoleTo(Sidekick);
        Sidekick targetRole = target.PrimaryRole<Sidekick>()!;
        targetRole.KillCooldown = KillCooldown;
        targetRole.SetParentJackal(this);
        targetRole.ImpostorVision = impostorVision; // I think imp vision is important enough for them to have.
        targetRole.CanVentOverride = SidekickGetsAbilities && canVent;
        targetRole.CanSabotageOverride = SidekickGetsAbilities && canSabotage;

        MyPlayer.NameModel().GetComponentHolder<RoleHolder>().ForEach(rc => rc.AddViewer(target));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub
                .KeyName("Can Vent", RoleTranslations.CanVent)
                .BindBool(b => canVent = b)
                .AddBoolean()
                .Build())
            .SubOption(sub => sub
                .KeyName("Can Sabotage", RoleTranslations.CanSabotage)
                .BindBool(b => canSabotage = b)
                .AddBoolean()
                .Build())
            .SubOption(sub => sub
                .KeyName("Impostor Vision", RoleTranslations.ImpostorVision)
                .BindBool(b => impostorVision = b)
                .AddBoolean()
                .Build())
            .SubOption(sub => sub
                .KeyName("Can Recruit", Translations.Options.CanRecruit)
                .BindBool(b => CanRecruit = b)
                .AddBoolean()
                .ShowSubOptionPredicate(v => (bool)v)
                .SubOption(sub2 => sub2
                    .KeyName("On Non-Crew Recruit", Translations.Options.OnNonCrewRecruit)
                    .Value(v => v.Value(0).Color(Color.red).Text(GeneralOptionTranslations.Kill).Build())
                    .Value(v => v.Value(1).Color(JackalColor).Text(Translations.Recruit).Build())
                    .BindInt(i => onNonCrewRecruit = (NonCrewRecruit)i)
                    .Build())
                .SubOption(sub2 => sub2
                    .KeyName("Kills Until Recruit", Translations.Options.KillsUntilRecruit)
                    .AddIntRange(0, ModConstants.MaxPlayers, 1)
                    .BindInt(i => killsForRecruit = i)
                    .Build())
                .SubOption(sub2 => sub2
                    .KeyName("Sidekick Becomes Jackal", Translations.Options.SidekickBecomesJackal)
                    .AddBoolean()
                    .BindBool(b => sidekickBecomesJackal = b)
                    .ShowSubOptionPredicate(v => (bool)v)
                    .SubOption(sub3 => sub3
                        .KeyName("New Jackal Can Recruit", Translations.Options.NewJackalCanRecruit)
                        .AddBoolean()
                        .BindBool(b => sidekickCanRecruit = b)
                        .Build())
                    .Build())
                .Build());

    public override List<CustomRole> LinkedRoles() => base.LinkedRoles().Concat([Sidekick]).ToList();

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(JackalColor)
            .CanVent(canVent)
            .Faction(JackalFaction.Instance)
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod(), () => !impostorVision);

    private enum NonCrewRecruit
    {
        Kill,
        Recruit
    }

    [Localized(nameof(Jackal))]
    public static class Translations
    {
        [Localized(nameof(NextKillIsRecruit))] public static string NextKillIsRecruit = "<size=1.5>You have enough kills for Recruit.\nYour next kill will recruit the player.</size>";
        [Localized(nameof(Recruit))] public static string Recruit = "Recruit";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(CanRecruit))] public static string CanRecruit = "Can Recruit";
            [Localized(nameof(OnNonCrewRecruit))] public static string OnNonCrewRecruit = "On Non-Crewmate Recruited";
            [Localized(nameof(KillsUntilRecruit))] public static string KillsUntilRecruit = "Kills Until Recruit";
            [Localized(nameof(SidekickBecomesJackal))] public static string SidekickBecomesJackal = "Sidekick becomes Jackal on Jackal Death";
            [Localized(nameof(NewJackalCanRecruit))] public static string NewJackalCanRecruit = "New Jackal Can Recruit";
        }
    }
}