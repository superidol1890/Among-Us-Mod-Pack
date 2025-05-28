using VentLib.Localization.Attributes;

namespace Lotus.Factions;

[Localized("Factions")]
public static class FactionTranslations
{
    [Localized(nameof(Crewmates))]
    public static class Crewmates
    {
        [Localized(nameof(Name))] public static string Name = "Crewmates";
        [Localized(nameof(Description))] public static string Description = "Complete tasks and vote out the killers.";
    }
    [Localized(nameof(Impostors))]
    public static class Impostors
    {
        [Localized(nameof(Name))] public static string Name = "Impostors";
        [Localized(nameof(Description))] public static string Description = "Kill all non-impostors.";
    }
    [Localized(nameof(Madmates))]
    public static class Madmates
    {
        [Localized(nameof(Name))] public static string Name = "Madmates";
        [Localized(nameof(Description))] public static string Description = "Help the impostors win.";
    }
    [Localized(nameof(TheUndead))]
    public static class TheUndead
    {
        [Localized(nameof(Name))] public static string Name = "The Undead";
        [Localized(nameof(Description))] public static string Description = "Kill everyone who oppose the Undead.";
    }
    [Localized(nameof(Neutral))]
    public static class Neutral
    {
        [Localized(nameof(Name))] public static string Name = "Neutral";
        [Localized(nameof(Description))] public static string Description = "Stay alive and complete your objective.";
    }
    [Localized(nameof(NeutralKillers))]
    public static class NeutralKillers
    {
        [Localized(nameof(Name))] public static string Name = "Neutral Killers";
        [Localized(nameof(Description))] public static string Description = "Kill everyone else and be the last one standing.";
    }
    [Localized(nameof(Modifiers))]
    public static class Modifiers
    {
        [Localized(nameof(Name))] public static string Name = "Modifiers";
        [Localized(nameof(Description))] public static string Description = "These roles add extra functionality to your base role.";
    }

    [Localized(nameof(ColorTeam))]
    public static class ColorTeam
    {
        [Localized(nameof(Name))] public static string Name = "Color Team";
        [Localized(nameof(Description))] public static string Description = "A team that is allies of everyone with the same color.";
    }
}