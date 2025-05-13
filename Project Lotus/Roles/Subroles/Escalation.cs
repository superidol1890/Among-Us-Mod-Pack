﻿using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities.Collections;

namespace Lotus.Roles.Subroles;

public class Escalation : Subrole
{
    private float speedGainPerKill;
    private int kills;

    private Remote<GameOptionOverride>? remote;

    public override string Identifier() => "◉";

    protected override void PostSetup()
    {
        AdditiveOverride additiveOverride = new(Override.PlayerSpeedMod, () => kills * speedGainPerKill);
        remote = Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, additiveOverride);
    }

    [RoleAction(LotusActionType.PlayerDeath, ActionFlag.GlobalDetector)]
    public void CheckPlayerDeath(PlayerControl target, PlayerControl killer, IDeathEvent deathEvent)
    {
        if (target.PlayerId == MyPlayer.PlayerId) remote?.Delete();
        killer = deathEvent.Instigator().Map(p => p.MyPlayer).OrElse(killer);
        if (killer.PlayerId != MyPlayer.PlayerId) return;

        kills++;
        killer.PrimaryRole().SyncOptions();
    }

    public override bool IsAssignableTo(PlayerControl player)
    {
        return player.PrimaryRole().RoleAbilityFlags.HasFlag(RoleAbilityFlag.IsAbleToKill) && base.IsAssignableTo(player);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Additional Speed per Kill", Translations.Options.SpeedGainPerKill)
                .AddFloatRange(0.1f, 1f, 0.1f, 3)
                .BindFloat(f => speedGainPerKill = f)
                .Build());

    protected override RoleType GetRoleType() => RoleType.Variation;

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleFlags(RoleFlag.VariationRole)
            .RoleColor(new Color(0.78f, 0.62f, 0.04f));

    [Localized(nameof(Escalation))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(SpeedGainPerKill))] public static string SpeedGainPerKill = "Additional Speed per Kill";
        }
    }
}