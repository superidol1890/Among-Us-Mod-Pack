using UnityEngine;
using VentLib.Utilities.Attributes;

namespace Lotus.GUI.Menus.OptionsMenu;

[LoadStatic]
internal class OptionMenuResources
{
    public static Sprite OptionsBackgroundSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(OptionsBackgroundSprite));
    public static Sprite ModUpdaterBackgroundSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ModUpdaterBackgroundSprite));

    public static Sprite ProgressBarFull => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ProgressBarFull));
    public static Sprite ProgressBarMask => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ProgressBarMask));


    public static Sprite QuestionHighlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(QuestionHighlight));
    public static Sprite QuestionInactive => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(QuestionInactive));

    public static Sprite ButtonOnSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ButtonOnSprite));
    public static Sprite ButtonOffSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ButtonOffSprite));

    public static Sprite GeneralButton_Highlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(GeneralButton_Highlight));
    public static Sprite LotusButton_Highlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(LotusButton_Highlight));
    public static Sprite GraphicsButton_Highlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(GraphicsButton_Highlight));
    public static Sprite VentLibButton_Highlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(VentLibButton_Highlight));
    public static Sprite AddonsButton_Highlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(AddonsButton_Highlight));
    public static Sprite InnerslothButton_Highlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(InnerslothButton_Highlight));
    public static Sprite ReturnButton_Highlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ReturnButton_Highlight));
    public static Sprite ExitButton_Highlight => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ExitButton_Highlight));

    public static Sprite GeneralButton_Inactive => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(GeneralButton_Inactive));
    public static Sprite LotusButton_Inactive => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(LotusButton_Inactive));
    public static Sprite GraphicsButton_Inactive => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(GraphicsButton_Inactive));
    public static Sprite VentLibButton_Inactive => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(VentLibButton_Inactive));
    public static Sprite AddonsButton_Inactive => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(AddonsButton_Inactive));
    public static Sprite InnerslothButton_Inactive => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(InnerslothButton_Inactive));
    public static Sprite Bottom_Inactive => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(Bottom_Inactive));


    const int ButtonPpu = 600;

    static OptionMenuResources()
    {
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(OptionsBackgroundSprite), "Lotus.assets.Settings.MenuBackground.png", 175);
        // PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(OptionsBackgroundSprite), "Lotus.assets.Settings.MenuGuideline.png", 175); // show guideline for now
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ModUpdaterBackgroundSprite), "Lotus.assets.Credits.Images.background.png", 400);

        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ProgressBarFull), "Lotus.assets.Settings.ProgressBarFill.png", 800);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ProgressBarMask), "Lotus.assets.Settings.ProgressBarMask.png", 800);

        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(QuestionHighlight), "Lotus.assets.Settings.QuestionHighlight.png", 100);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(QuestionInactive), "Lotus.assets.Settings.QuestionInactive.png", 100);

        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ButtonOnSprite), "Lotus.assets.Settings.SelectHighlight.png", 450);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ButtonOffSprite), "Lotus.assets.Settings.SelectInactive.png", 450);

        // HIGHLIGHT BUTTONS
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(GeneralButton_Highlight), "Lotus.assets.Settings.GeneralHighlight.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(LotusButton_Highlight), "Lotus.assets.Settings.LotusHighlight.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(GraphicsButton_Highlight), "Lotus.assets.Settings.GraphicsHighlight.png", ButtonPpu);
        // PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(SoundButton_Highlight), "Lotus.assets.Settings.SoundHighlight.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(VentLibButton_Highlight), "Lotus.assets.Settings.VentlibHighlight.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(AddonsButton_Highlight), "Lotus.assets.Settings.AddonsHighlight.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(InnerslothButton_Highlight), "Lotus.assets.Settings.InnerslothHighlight.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ReturnButton_Highlight), "Lotus.assets.Settings.ReturnHighlight.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ExitButton_Highlight), "Lotus.assets.Settings.LeaveLobbyHighlight.png", ButtonPpu);

        // INACTIVE BUTTONS
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(GeneralButton_Inactive), "Lotus.assets.Settings.GeneralInactive.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(LotusButton_Inactive), "Lotus.assets.Settings.LotusInactive.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(GraphicsButton_Inactive), "Lotus.assets.Settings.GraphicsInactive.png", ButtonPpu);
        // PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(SoundButton_Inactive), "Lotus.assets.Settings.SoundInactive.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(VentLibButton_Inactive), "Lotus.assets.Settings.VentlibInactive.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(AddonsButton_Inactive), "Lotus.assets.Settings.AddonsInactive.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(InnerslothButton_Inactive), "Lotus.assets.Settings.InnerslothInactive.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(Bottom_Inactive), "Lotus.assets.Settings.BottomInactive.png", ButtonPpu);
    }

}