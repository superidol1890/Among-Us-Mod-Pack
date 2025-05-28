using AmongUs.GameOptions;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Phantom : Impostor
{
    protected float VanishCooldown;
    protected float VanishDuration;

    [RoleAction(LotusActionType.Attack, Subclassing = false)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    protected GameOptionBuilder AddVanishOptions(GameOptionBuilder builder)
    {
        return builder.SubOption(sub => sub.KeyName("Vanish Cooldown", Translations.Options.VanishCooldown)
                .AddFloatRange(0, 120, 2.5f, 12, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => VanishCooldown = f)
                .Build())
            .SubOption(sub => sub.KeyName("Vanish Duration", Translations.Options.VanishDuration)
                .Value(1f)
                .AddFloatRange(2.5f, 120, 2.5f, 6, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => VanishDuration = f)
                .Build());
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream)
    {
        try
        {
            var callingMethod = Mirror.GetCaller();
            var callingType = callingMethod?.DeclaringType;

            if (callingType == null)
            {
                return base.RegisterOptions(optionStream);
            }
            if (callingType == typeof(AbstractBaseRole)) return AddVanishOptions(base.RegisterOptions(optionStream));
            else return base.RegisterOptions(optionStream);
        }
        catch
        {
            return base.RegisterOptions(optionStream);
        }
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Phantom)
            .RoleColor(Color.red)
            .CanVent(true)
            .OptionOverride(Override.PhantomVanishCooldown, () => VanishCooldown)
            .OptionOverride(Override.PhantomVanishDuration, () => VanishDuration);

    [Localized(nameof(Phantom))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(VanishCooldown))]
            public static string VanishCooldown = "Vanish Cooldown";

            [Localized(nameof(VanishDuration))]
            public static string VanishDuration = "Vanish Duration";
        }
    }
}