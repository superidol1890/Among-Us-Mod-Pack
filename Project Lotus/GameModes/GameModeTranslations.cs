using VentLib.Localization.Attributes;

namespace Lotus.GameModes;

[Localized("Options.GameMode")]
public class GamemodeTranslations
{
    [Localized(nameof(GamemodeText))] public static string GamemodeText = "GameMode";
    [Localized(nameof(GamemodeSelection))] public static string GamemodeSelection = "Gamemode Selection";


    [Localized(nameof(Standard))]
    public class Standard
    {
        [Localized(nameof(Name))] public static string Name = "Standard";

        [Localized(nameof(ButtonText))] public static string ButtonText = "Lotus Settings";
        [Localized(nameof(Description))] public static string Description = "Modify all the Standard settings here!";

        [Localized(nameof(ImpostorTab))] public static string ImpostorTab = "Impostor Settings";
        [Localized(nameof(CrewmateTab))] public static string CrewmateTab = "Crewmate Settings";
        [Localized(nameof(NeutralTab))] public static string NeutralTab = "Neutral Settings";
        [Localized(nameof(MiscTab))] public static string MiscTab = "Misc Settings";
        [Localized(nameof(HiddenTab))] public static string HiddenTab = "Hidden";

        [Localized(nameof(GamemodeDescription))] public static string GamemodeDescription = "This text is shown to players when they join to explain the current gamemode.";
    }

    [Localized(nameof(CaptureTheFlag))]
    public class CaptureTheFlag
    {
        [Localized(nameof(Name))] public static string Name = "Capture The Flag";

        [Localized(nameof(ButtonText))] public static string ButtonText = "CTF Settings";
        [Localized(nameof(Description))] public static string Description = "Change anything about CTF here!";

        [Localized(nameof(GamemodeDescription))] public static string GamemodeDescription = "This text is shown to players when they join to explain the current gamemode.";
    }

    [Localized(nameof(Colorwars))]
    public class Colorwars
    {
        [Localized(nameof(Name))] public static string Name = "Colorwars";

        [Localized(nameof(ButtonText))] public static string ButtonText = "Colorwars Settings";
        [Localized(nameof(Description))] public static string Description = "Modify the Colorwars settings here!";

        [Localized(nameof(GamemodeDescription))] public static string GamemodeDescription = "This text is shown to players when they join to explain the current gamemode.";
    }
}