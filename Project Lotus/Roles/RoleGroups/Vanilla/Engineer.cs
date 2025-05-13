using AmongUs.GameOptions;
using Lotus.Options;
using Lotus.Roles.Overrides;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Engineer : Crewmate
{
    protected float VentCooldown;
    protected float VentDuration;

    protected GameOptionBuilder AddVentingOptions(GameOptionBuilder builder)
    {
        return builder.SubOption(sub => sub
                .Key("Vent Cooldown")
                .Name(EngineerTranslations.Options.VentCooldown)
                .AddFloatRange(0, 120, 2.5f, 16, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => VentCooldown = f)
                .Build())
            .SubOption(sub => sub.Name(EngineerTranslations.Options.VentDuration)
                .Key("Vent Duration")
                .Value(1f)
                .AddFloatRange(2.5f, 120, 2.5f, 6, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => VentDuration = f)
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
            if (callingType == typeof(AbstractBaseRole)) return AddVentingOptions(base.RegisterOptions(optionStream));
            else return base.RegisterOptions(optionStream);
        }
        catch
        {
            return base.RegisterOptions(optionStream);
        }
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .CanVent(true)
            .VanillaRole(RoleTypes.Engineer)
            .OptionOverride(Override.EngVentCooldown, () => VentCooldown)
            .OptionOverride(Override.EngVentDuration, () => VentDuration);

    [Localized(nameof(Engineer))]
    public static class EngineerTranslations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(VentCooldown))]
            public static string VentCooldown = "Vent Cooldown";

            [Localized(nameof(VentDuration))]
            public static string VentDuration = "Vent Duration";
        }
    }
}