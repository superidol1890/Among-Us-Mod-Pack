using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.UI;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using Lotus.Extensions;
using Lotus.Logging;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Impostors;

public class BountyHunter : Impostor
{
    private Cooldown acquireNewTarget = null!;
    private FrozenPlayer? bountyTarget;

    private float bountyKillCoolDown;
    private float punishKillCoolDown;

    private IRemote? cooldownOverride;

    [UIComponent(UI.Text)]
    private string ShowTarget() => Color.red.Colorize("Target: ") + Color.white.Colorize(bountyTarget == null ? "None" : bountyTarget.Name);

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        SendKillCooldown(bountyTarget?.PlayerId == target.PlayerId);
        bool success = base.TryKill(target);
        if (success)
            BountyHunterAcquireTarget();
        return success;
    }

    [RoleAction(LotusActionType.FixedUpdate)]
    private void BountyHunterTargetUpdate()
    {
        if (acquireNewTarget.NotReady()) return;
        BountyHunterAcquireTarget();
    }

    [RoleAction(LotusActionType.RoundStart)]
    private void BountyHunterTargetOnRoundStart() => BountyHunterAcquireTarget();

    private void BountyHunterAcquireTarget()
    {
        List<PlayerControl> eligiblePlayers = Players.GetAlivePlayers()
            .Where(p => p.PlayerId != MyPlayer.PlayerId && p.Relationship(MyPlayer) is not Relation.FullAllies)
            .ToList();
        if (eligiblePlayers.Count == 0)
        {
            bountyTarget = null;
            return;
        }

        // Small function to assign a NEW random target unless there's only one eligible target alive
        PlayerControl newTarget = eligiblePlayers.PopRandom();
        while (eligiblePlayers.Count > 1 && bountyTarget?.PlayerId == newTarget.PlayerId)
            newTarget = eligiblePlayers.PopRandom();

        bountyTarget = Game.MatchData.GetFrozenPlayer(newTarget);
        acquireNewTarget.Start();
    }

    private void SendKillCooldown(bool decreased)
    {
        float cooldown = decreased ? bountyKillCoolDown : punishKillCoolDown;
        cooldownOverride?.Delete();
        cooldownOverride = AddOverride(new GameOptionOverride(Override.KillCooldown, cooldown));
        SyncOptions();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Color(RoleColor)
            .SubOption(sub => sub
                .KeyName("Time Until New Target", Translations.Options.TimeUntilNewTarget)
                .Bind(v => acquireNewTarget.Duration = (float)v)
                .AddFloatRange(5f, 120, 5, 11)
                .IOSettings(settings => settings.UnknownValueAction = ADEAnswer.Allow)
                .Build())
            .SubOption(sub => sub
                .KeyName("Kill Cooldown After Killing Target", Translations.Options.AfterKillingTarget)
                .Bind(v => bountyKillCoolDown = (float)v)
                .AddFloatRange(0, 180, 2.5f, 6)
                .IOSettings(settings => settings.UnknownValueAction = ADEAnswer.Allow)
                .Build())
            .SubOption(sub => sub
                .KeyName("Kill Cooldown After Killing Other", Translations.Options.AfterKillingNonTarget)
                .Bind(v => punishKillCoolDown = (float)v)
                .AddFloatRange(0, 180, 2.5f, 15)
                .IOSettings(settings => settings.UnknownValueAction = ADEAnswer.Allow)
                .Build());

    [Localized(nameof(BountyHunter))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(TimeUntilNewTarget))]
            public static string TimeUntilNewTarget = "Time Until New Target";

            [Localized(nameof(AfterKillingTarget))]
            public static string AfterKillingTarget = "Kill Cooldown After Killing Target";

            [Localized(nameof(AfterKillingNonTarget))]
            public static string AfterKillingNonTarget = "Kill Cooldown After Killing Non-Target";
        }
    }
}