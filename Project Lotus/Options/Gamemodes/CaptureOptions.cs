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
public class CaptureOptions
{
    public int GameLength;
    public int ReviveDuration;

    public float KillCooldown;
    public float CarryingSpeedMultiplier;

    public bool CanVent;
    public bool CarryingCanVent;

    public List<GameOption> AllOptions = new();

    public CaptureOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(Translations.CaptureTitle)
            .IsHeader(false)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Game Length", Translations.GameLength)
            .AddIntRange(60, 600, 15, 4, GeneralOptionTranslations.SecondsSuffix)
            .IsHeader(true)
            .BindInt(i => GameLength = i)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Revive Timeout", Translations.ReviveDuration)
            .AddIntRange(1, 30, 1, 4, GeneralOptionTranslations.SecondsSuffix)
            .BindInt(i => ReviveDuration = i)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Kill Cooldown", RoleTranslations.KillCooldown)
            .AddFloatRange(2.5f, 60f, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
            .BindFloat(f => KillCooldown = f)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Can Vent", RoleTranslations.CanVent)
            .AddBoolean()
            .BindBool(b => CanVent = b)
            .ShowSubOptionPredicate(o => (bool)o)
            .SubOption(sub => sub
                .KeyName("Carrying Can Vent", Translations.CarryingCanVent)
                .AddBoolean(false)
                .BindBool(b => CarryingCanVent = b)
                .Build())
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .KeyName("Carrying Speed Multiplier", Translations.CarryingSpeedMultiplier)
            .AddFloatRange(0.2f, 1f, .05f, 16, "x")
            .BindFloat(f => CarryingSpeedMultiplier = f)
            .Build());
    }

    [Localized("CaptureTheFlag")]
    public class Translations
    {
        [Localized("SectionTitle")] public static string CaptureTitle = "CTF Settings";

        [Localized(nameof(GameLength))] public static string GameLength = "Game Duration";
        [Localized(nameof(ReviveDuration))] public static string ReviveDuration = "Revive Timeout";
        [Localized(nameof(CarryingSpeedMultiplier))] public static string CarryingSpeedMultiplier = "Carrying Speed Multiplier";
        [Localized(nameof(CarryingCanVent))] public static string CarryingCanVent = "Carrying Can Vent";
    }
}
