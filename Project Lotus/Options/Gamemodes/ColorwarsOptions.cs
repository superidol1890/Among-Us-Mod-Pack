using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lotus.Extensions;
using Lotus.Roles;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;

namespace Lotus.Options.Gamemodes;

[Localized(ModConstants.Options)]
public class ColorwarsOptions
{
    public int TeamSize;

    public float GracePeriod;
    public float KillCooldown;

    public bool ConvertColorMode;
    public bool PreferSmall;
    public bool RandomSpawn;
    public bool CustomTeams;
    public bool CanVent;

    public GameOption CustomTeamsOption;

    public List<GameOption> AllOptions = new();

    public ColorwarsOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(Translations.ColorwarsTitle)
            .IsHeader(false)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Team Size", Translations.TeamSize)
            .IsHeader(true)
            .BindInt(i => TeamSize = i)
            .AddIntRange(1, 8, 1, 2)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Kill Cooldown", RoleTranslations.KillCooldown)
            .IsHeader(true)
            .AddFloatRange(2.5f, 60f, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
            .BindFloat(v => KillCooldown = v)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
           .KeyName("Grace Period", Translations.GracePeriod)
           .IsHeader(true)
           .AddFloatRange(0, 30, 1f, 5, GeneralOptionTranslations.SecondsSuffix)
           .BindFloat(v => GracePeriod = v)
           .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Prefer Small", Translations.PreferSmall)
            .IsHeader(true)
            .AddBoolean()
            .BindBool(b => PreferSmall = b)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Convert Mode", Translations.ConvertColorMode)
            .IsHeader(true)
            .AddBoolean()
            .BindBool(b => ConvertColorMode = b)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .LocaleName($"{ModConstants.Options}.Mayhem.RandomSpawn")
            .Key("Random Spawn")
            .AddBoolean()
            .BindBool(b => RandomSpawn = b)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Can Vent", RoleTranslations.CanVent)
            .AddBoolean()
            .BindBool(b => CanVent = b)
            .Build());

        CustomTeamsOption = new GameOptionBuilder()
            .KeyName("Custom Teams", Translations.CustomTeams)
            .AddBoolean(false)
            .BindBool(b => CustomTeams = b)
            .ShowSubOptionPredicate(v => (bool)v)
            .Build();

        AllOptions.Add(CustomTeamsOption);
    }

    [Localized("Colorwars")]
    public class Translations
    {
        [Localized("SectionTitle")] public static string ColorwarsTitle = "Colorwars Settings";
        [Localized(nameof(CustomTeamTitle))] public static string CustomTeamTitle = "Custom Teams";

        [Localized(nameof(TeamSize))] public static string TeamSize = "Team Size";
        [Localized(nameof(PreferSmall))] public static string PreferSmall = "Prefer Smaller Teams";
        [Localized(nameof(ConvertColorMode))] public static string ConvertColorMode = "Convert Color Mode";
        [Localized(nameof(GracePeriod))] public static string GracePeriod = "Grace Period";
        [Localized(nameof(CustomTeams))] public static string CustomTeams = "Custom Teams";

        [Localized(nameof(TeamOption))] public static string TeamOption = "Team {0}";
    }
}
