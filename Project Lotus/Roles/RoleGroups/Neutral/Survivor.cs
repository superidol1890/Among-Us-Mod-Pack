using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Victory;
using Lotus.Victory.Conditions;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Survivor : CustomRole
{
    [UIComponent(UI.Cooldown)]
    private Cooldown vestCooldown;
    private Cooldown vestDuration;

    private int vestUsages;
    private int remainingVests;

    [UIComponent(UI.Counter)]
    private string VestCounter() => vestUsages == -1 ? "" : RoleUtils.Counter(remainingVests, vestUsages, RoleColor);

    [UIComponent(UI.Indicator)]
    private string GetVestString() => vestDuration.IsReady() ? "" : RoleColor.Colorize("♣");


    protected override void PostSetup()
    {
        remainingVests = vestUsages;
        Game.GetWinDelegate().AddSubscriber(GameEnd);
    }

    [RoleAction(LotusActionType.Interaction)]
    private void SurvivorProtection(Interaction interaction, ActionHandle handle)
    {
        if (vestDuration.IsReady()) return;
        if (interaction.Intent is not IFatalIntent) return;
        handle.Cancel();
    }

    [RoleAction(LotusActionType.OnPet)]
    public void OnPet()
    {
        if (remainingVests == 0 || vestDuration.NotReady() || vestCooldown.NotReady()) return;
        remainingVests--;
        vestDuration.StartThenRun(() => vestCooldown.Start()); ;
    }

    private void GameEnd(WinDelegate winDelegate)
    {
        if (!MyPlayer.IsAlive() || winDelegate.GetWinReason().ReasonType is ReasonType.SoloWinner) return;
        winDelegate.AddAdditionalWinner(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub
                .KeyName("Vest Duration", Translations.Options.VestDuration)
                .BindFloat(vestDuration.SetDuration)
                .AddFloatRange(2.5f, 180f, 2.5f, 11, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Vest Cooldown", Translations.Options.VestCooldown)
                .BindFloat(vestCooldown.SetDuration)
                .AddFloatRange(2.5f, 180f, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Number of Shields", Translations.Options.TotalShields)
                .BindInt(i => vestUsages = i)
                .Value(v => v.Value(-1).Text("∞").Color(ModConstants.Palette.InfinityColor).Build())
                .AddIntRange(1, 60, 1)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier
            .Faction(FactionInstances.Neutral)
            .SpecialType(SpecialType.Neutral)
            .RoleAbilityFlags(RoleAbilityFlag.UsesPet)
            .RoleColor(new Color(1f, 0.9f, 0.3f));

    [Localized(nameof(Survivor))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(VestDuration))]
            public static string VestDuration = "Shield Duration";

            [Localized(nameof(VestCooldown))]
            public static string VestCooldown = "Shield Cooldown";

            [Localized(nameof(TotalShields))]
            public static string TotalShields = "Number of Shields";
        }
    }
}