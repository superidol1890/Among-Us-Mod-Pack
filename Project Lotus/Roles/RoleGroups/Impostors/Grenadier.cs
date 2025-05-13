using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Grenadier : Vanilla.Impostor
{
    [UIComponent(UI.Cooldown)]
    private Cooldown blindCooldown;
    private float blindDuration;
    private float blindDistance;
    private bool canVent;
    private bool canBlindAllies;
    private int grenadeAmount;
    private int grenadesLeft;

    [RoleAction(LotusActionType.Attack)]
    public new bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(LotusActionType.OnPet)]
    private void GrenadierBlind()
    {
        if (blindCooldown.NotReady() || grenadesLeft <= 0) return;

        GameOptionOverride[] overrides = { new(Override.CrewLightMod, 0f), new(Override.ImpostorLightMod, 0f) };
        List<PlayerControl> playersInDistance = blindDistance > 0
            ? RoleUtils.GetPlayersWithinDistance(MyPlayer, blindDistance).ToList()
            : MyPlayer.GetPlayersInAbilityRangeSorted();

        playersInDistance.Where(p => canBlindAllies || p.Relationship(MyPlayer) is not Relation.FullAllies)
            .Do(p =>
            {
                p.PrimaryRole().SyncOptions(overrides);
                Async.Schedule(() => p.PrimaryRole().SyncOptions(), blindDuration);
            });

        blindCooldown.Start();
        grenadesLeft--;
    }

    [RoleAction(LotusActionType.RoundStart)]
    private void SetGrenadeAmount() => grenadesLeft = grenadeAmount;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Amount of Grenades", Translations.Options.AmountOfGrenades)
                .Bind(v => grenadeAmount = (int)v)
                .AddIntRange(1, 5, 1, 2)
                .Build())
            .SubOption(sub => sub
                .KeyName("Blind Cooldown", Translations.Options.BlindCooldown)
                .Bind(v => blindCooldown.Duration = (float)v)
                .AddFloatRange(5f, 120f, 2.5f, 10, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Blind Duration", Translations.Options.BlindDuration)
                .Bind(v => blindDuration = (float)v)
                .AddFloatRange(5f, 60f, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Blind Effect Radius", Translations.Options.BlindRadius)
                .Bind(v => blindDistance = (float)v)
                .Value(v => v.Text("Kill Distance").Value(-1f).Build())
                .AddFloatRange(1.5f, 3f, 0.1f, 4)
                .Build())
            .SubOption(sub => sub
                .KeyName("Can Blind Allies", Translations.Options.CanBlindAllies)
                .Bind(v => canBlindAllies = (bool)v)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .KeyName("Can Vent", RoleTranslations.CanVent)
                .Bind(v => canVent = (bool)v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .CanVent(canVent)
            .RoleAbilityFlags(RoleAbilityFlag.UsesPet);

    [Localized(nameof(Grenadier))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(AmountOfGrenades))]
            public static string AmountOfGrenades = "Amount of Grenades";

            [Localized(nameof(BlindCooldown))]
            public static string BlindCooldown = "Blind Cooldown";

            [Localized(nameof(BlindDuration))]
            public static string BlindDuration = "Blind Duration";

            [Localized(nameof(BlindRadius))]
            public static string BlindRadius = "Blind Effect Radius";

            [Localized(nameof(CanBlindAllies))]
            public static string CanBlindAllies = "Can Blind Allies";
        }
    }
}