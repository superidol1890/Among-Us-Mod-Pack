using System.Collections.Generic;
using Lotus.GameModes;
using Lotus.Utilities;
using VentLib.Options.UI;
using VentLib.Options.UI.Controllers;
using VentLib.Options.UI.Tabs;
using VentLib.Utilities.Attributes;

namespace Lotus.Options;

[LoadStatic]
public class DefaultTabs
{
    public static MainSettingTab StandardTab = new(GamemodeTranslations.Standard.ButtonText, GamemodeTranslations.Standard.Description);
    public static MainSettingTab CaptureTab = new(GamemodeTranslations.CaptureTheFlag.ButtonText, GamemodeTranslations.CaptureTheFlag.Description);
    public static MainSettingTab ColorwarsTab = new(GamemodeTranslations.Colorwars.ButtonText, GamemodeTranslations.Colorwars.Description);

    public static GameOptionTab ImpostorsTab = new(GamemodeTranslations.Standard.ImpostorTab, () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.ImpostorsIconNew.png", 270, true));

    public static GameOptionTab CrewmateTab = new(GamemodeTranslations.Standard.CrewmateTab, () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.CrewmatesIconNew.png", 270, true));

    public static GameOptionTab NeutralTab = new(GamemodeTranslations.Standard.NeutralTab, () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.NeutralsIconNew.png", 270, true));

    public static GameOptionTab MiscTab = new(GamemodeTranslations.Standard.MiscTab, () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.MiscIconNew.png", 270, true));

    public static GameOptionTab HiddenTab = new(GamemodeTranslations.Standard.HiddenTab, () => AssetLoader.LoadSprite("Lotus.assets.GeneralIcon.png", 300, true));

    public static List<GameOptionTab> StandardTabs = new() { ImpostorsTab, CrewmateTab, NeutralTab, MiscTab };
    public static List<GameOptionTab> CaptureTabs = new();
    public static List<GameOptionTab> ColorwarsTabs = new();

    static DefaultTabs()
    {
        // GameOptionController.AddTab(GeneralTab);
        // GameOptionController.AddTab(ImpostorsTab);
        // GameOptionController.AddTab(CrewmateTab);
        // GameOptionController.AddTab(NeutralTab);
        // GameOptionController.AddTab(MiscTab);
        SettingsOptionController.SetMainTab(StandardTab);
    }
}