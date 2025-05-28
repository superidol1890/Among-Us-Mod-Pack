using UnityEngine;
using VentLib.Utilities.Attributes;

namespace Lotus.GUI.Menus.OptionsMenu;

// [LoadStatic]
internal class OptionMenuResources
{
    const int ButtonPpu = 100;

    public static Sprite OptionsBackgroundSprite => LotusAssets.LoadSprite("Settings/MenuBackground.png", 175);
    public static Sprite ModUpdaterBackgroundSprite => LotusAssets.LoadSprite("Credits/Images/background.png", 400);

    public static Sprite ProgressBarFull => LotusAssets.LoadSprite("Settings/ProgressBarFill.png", 800);
    public static Sprite ProgressBarMask => LotusAssets.LoadSprite("Settings/ProgressBarMask.png", 800);


    public static Sprite QuestionHighlight => LotusAssets.LoadSprite("Settings/QuestionHighlight.png");
    public static Sprite QuestionInactive => LotusAssets.LoadSprite("Settings/QuestionInactive.png");

    public static Sprite ButtonOnSprite => LotusAssets.LoadSprite("Settings/SelectHighlight.png", 450);
    public static Sprite ButtonOffSprite => LotusAssets.LoadSprite("Settings/SelectInactive.png", 450);

    public static Sprite GeneralButton_Highlight => LotusAssets.LoadSprite("Settings/GeneralHighlight.png", ButtonPpu);
    public static Sprite LotusButton_Highlight => LotusAssets.LoadSprite("Settings/LotusHighlight.png", ButtonPpu);
    public static Sprite GraphicsButton_Highlight => LotusAssets.LoadSprite("Settings/GraphicsHighlight.png", ButtonPpu);
    public static Sprite VentLibButton_Highlight => LotusAssets.LoadSprite("Settings/VentlibHighlight.png", ButtonPpu);
    public static Sprite AddonsButton_Highlight => LotusAssets.LoadSprite("Settings/AddonsHighlight.png", ButtonPpu);
    public static Sprite InnerslothButton_Highlight => LotusAssets.LoadSprite("Settings/InnerslothHighlight.png", ButtonPpu);
    public static Sprite ReturnButton_Highlight => LotusAssets.LoadSprite("Settings/ReturnHighlight.png", ButtonPpu);
    public static Sprite ExitButton_Highlight => LotusAssets.LoadSprite("Settings/LeaveLobbyHighlight.png", ButtonPpu);

    public static Sprite GeneralButton_Inactive => LotusAssets.LoadSprite("Settings/GeneralInactive.png", ButtonPpu);
    public static Sprite LotusButton_Inactive => LotusAssets.LoadSprite("Settings/LotusInactive.png", ButtonPpu);
    public static Sprite GraphicsButton_Inactive => LotusAssets.LoadSprite("Settings/GraphicsInactive.png", ButtonPpu);
    public static Sprite VentLibButton_Inactive => LotusAssets.LoadSprite("Settings/VentlibInactive.png", ButtonPpu);
    public static Sprite AddonsButton_Inactive => LotusAssets.LoadSprite("Settings/AddonsInactive.png", ButtonPpu);
    public static Sprite InnerslothButton_Inactive => LotusAssets.LoadSprite("Settings/InnerslothInactive.png", ButtonPpu);
    public static Sprite Bottom_Inactive => LotusAssets.LoadSprite("Settings/BottomInactive.png", ButtonPpu);

}