using System.Diagnostics;
using AmongUs.GameOptions;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Shapeshifter : Impostor
{
    protected float ShapeshiftCooldown;
    protected float ShapeshiftDuration;

    [RoleAction(LotusActionType.Attack, Subclassing = false)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    protected GameOptionBuilder AddShapeshiftOptions(GameOptionBuilder builder)
    {
        return builder.SubOption(sub => sub.KeyName("Shapeshift Cooldown", Translations.Options.ShapeshiftCooldown)
                .AddFloatRange(0, 120, 2.5f, 12, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => ShapeshiftCooldown = f)
                .Build())
            .SubOption(sub => sub.KeyName("Shapeshift Duration", Translations.Options.ShapeshiftDuration)
                .Value(1f)
                .AddFloatRange(2.5f, 120, 2.5f, 6, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => ShapeshiftDuration = f)
                .Build());
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream)
    {
        try
        {
            var callingMethod = new StackTrace().GetFrame(1)?.GetMethod();
            var callingType = callingMethod?.DeclaringType;

            if (callingType == null)
            {
                return base.RegisterOptions(optionStream);
            }
            if (callingType == typeof(AbstractBaseRole)) return AddShapeshiftOptions(base.RegisterOptions(optionStream));
            else return base.RegisterOptions(optionStream);
        }
        catch
        {
            return base.RegisterOptions(optionStream);
        }
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Shapeshifter)
            .RoleColor(Color.red)
            .CanVent(true)
            .OptionOverride(Override.ShapeshiftCooldown, () => ShapeshiftCooldown)
            .OptionOverride(Override.ShapeshiftDuration, () => ShapeshiftDuration);

    [Localized(nameof(Shapeshifter))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(ShapeshiftCooldown))]
            public static string ShapeshiftCooldown = "Shapeshift Cooldown";

            [Localized(nameof(ShapeshiftDuration))]
            public static string ShapeshiftDuration = "Shapeshift Duration";
        }
    }
}