using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lotus.GUI.Menus.OptionsMenu.Components;
using Lotus.GUI.Menus.OptionsMenu.Submenus;
using Lotus.Logging;
using Lotus.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
// ReSharper disable InconsistentNaming

namespace Lotus.GUI.Menus.OptionsMenu;

[Localized("GUI")]
[RegisterInIl2Cpp]
public class CustomOptionContainer : MonoBehaviour
{
    [Localized(nameof(GeneralButton))] private static string GeneralButton = "General";
    [Localized(nameof(LotusButton))] private static string LotusButton = "Lotus";
    [Localized(nameof(GraphicsButton))] private static string GraphicsButton = "Graphics";
    [Localized(nameof(VentLibButton))] private static string VentLibButton = "VentLib";
    [Localized(nameof(AddonsButton))] private static string AddonsButton = "Addons";
    [Localized(nameof(InnerslothButton))] private static string InnerslothButton = "Innersloth";

    [Localized(nameof(ReturnButton))] private static string ReturnButton = "Return";
    [Localized(nameof(LeaveGameButton))] private static string LeaveGameButton = "Leave Game";


    public static UnityOptional<TMP_FontAsset> CustomOptionFont = UnityOptional<TMP_FontAsset>.Null();

    public SpriteRenderer background;

    public PassiveButton generalButton;
    public PassiveButton lotusButton;
    public PassiveButton graphicButton;
    public PassiveButton ventLibButton;
    public PassiveButton addonsButton;
    public PassiveButton innerslothButton;

    public PassiveButton returnButton;
    public PassiveButton exitButton;

    private GeneralMenu generalMenu;
    private LotusMenu lotusMenu;
    private GraphicsMenu graphicsMenu;
    private VentLibMenu ventLibMenu;
    private AddonsMenu addonsMenu;
    private InnerslothMenu innerslothMenu;

    // private SoundMenu soundMenu;

    private PassiveButton? selectedButton;
    private PassiveButton? template;
    private const int ButtonPpu = 600;

    private List<(PassiveButton, IBaseOptionMenuComponent)> boundButtons = new();

    public CustomOptionContainer(IntPtr intPtr) : base(intPtr)
    {
        transform.localPosition += new Vector3(1f, 0f);
        background = gameObject.AddComponent<SpriteRenderer>();
        background.sprite = OptionMenuResources.OptionsBackgroundSprite;

        generalMenu = gameObject.AddComponent<GeneralMenu>();
        lotusMenu = gameObject.AddComponent<LotusMenu>();
        graphicsMenu = gameObject.AddComponent<GraphicsMenu>();
        ventLibMenu = gameObject.AddComponent<VentLibMenu>();
        addonsMenu = gameObject.AddComponent<AddonsMenu>();
        innerslothMenu = gameObject.AddComponent<InnerslothMenu>();
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        generalMenu.PassMenu(menuBehaviour);
        lotusMenu.PassMenu(menuBehaviour);
        graphicsMenu.PassMenu(menuBehaviour);
        ventLibMenu.PassMenu(menuBehaviour);
        addonsMenu.PassMenu(menuBehaviour);
        innerslothMenu.PassMenu(menuBehaviour);

        ButtonCreator buttonCreator = CreateButton(menuBehaviour);
        Func<Sprite, Sprite, string, PassiveButton> buttonFunc = (inactive, active, text) => buttonCreator(inactive, active, text);

        // CREATE BUTTONS

        generalButton = buttonFunc(OptionMenuResources.GeneralButton_Inactive, OptionMenuResources.GeneralButton_Highlight, GeneralButton);
        generalButton.transform.localPosition = new Vector3(-3.386f, 2.033f, -1f);

        lotusButton = buttonFunc(OptionMenuResources.LotusButton_Inactive, OptionMenuResources.LotusButton_Highlight, LotusButton);
        lotusButton.transform.localPosition = new Vector3(-3.386f, 1.383f, -1f);

        graphicButton = buttonFunc(OptionMenuResources.GraphicsButton_Inactive, OptionMenuResources.GraphicsButton_Highlight, GraphicsButton);
        graphicButton.transform.localPosition = new Vector3(-3.386f, 0.7265f, -1f);

        ventLibButton = buttonFunc(OptionMenuResources.VentLibButton_Inactive, OptionMenuResources.VentLibButton_Highlight, VentLibButton);
        ventLibButton.transform.localPosition = new Vector3(-3.386f, 0.0798f, -1f);

        addonsButton = buttonFunc(OptionMenuResources.AddonsButton_Inactive, OptionMenuResources.AddonsButton_Highlight, AddonsButton);
        addonsButton.transform.localPosition = new Vector3(-3.386f, -0.5778f, -1f);

        innerslothButton = buttonFunc(OptionMenuResources.InnerslothButton_Inactive, OptionMenuResources.InnerslothButton_Highlight, InnerslothButton);
        innerslothButton.transform.localPosition = new Vector3(-3.386f, -1.2295f, -1f);

        // FINISH BUTTONS

        returnButton = buttonFunc(OptionMenuResources.Bottom_Inactive, OptionMenuResources.ReturnButton_Highlight, ReturnButton);
        returnButton.GetComponentInChildren<TextMeshPro>().transform.localPosition = new Vector3(0, -.01f, 0f);
        returnButton.GetComponentInChildren<TextMeshPro>().transform.localScale = new Vector3(2.5f, 0.8f, 1f);
        returnButton.transform.localPosition = new Vector3(-3.87f, -2.67f, -1f);
        returnButton.transform.localScale = new Vector3(0.3764f, 1.4f, 1);
        returnButton.OnClick.AddListener((Action)menuBehaviour.Close);

        exitButton = buttonFunc(OptionMenuResources.Bottom_Inactive, OptionMenuResources.ExitButton_Highlight, LeaveGameButton);
        exitButton.GetComponentInChildren<TextMeshPro>().transform.localPosition = new Vector3(0, -.01f, -1);
        exitButton.GetComponentInChildren<TextMeshPro>().transform.localScale = new Vector3(1.8f, 0.9f, 1f);
        exitButton.transform.localPosition = new Vector3(-2.905f, -2.67f, -1f);
        exitButton.transform.localScale = new Vector3(0.3764f, 1.4f, 1f);

        menuBehaviour.FindChildOrEmpty<PassiveButton>("LeaveGameButton").Handle(exitPassiveButton =>
        {
            exitPassiveButton.gameObject.SetActive(false);
            exitButton.OnClick.AddListener((Action)exitPassiveButton.ReceiveClickDown);
        }, () =>
        {
            exitButton.gameObject.SetActive(false);
            returnButton.gameObject.SetActive(false);
        });

        menuBehaviour.GetComponentsInChildren<PassiveButton>().Where(pb => pb.name.Contains("Background")).ForEach(pb => pb.OnClick = new Button.ButtonClickedEvent());
        menuBehaviour.Background.enabled = false;

        menuBehaviour.Tabs.ForEach(t => t.gameObject.SetActive(false));
        menuBehaviour.Tabs[0].Content.SetActive(false);
        menuBehaviour.Tabs[0].Content.transform.localPosition += new Vector3(0f, 1000f);

        menuBehaviour.BackButton.transform.localPosition += new Vector3(-1.2f, 0.17f);
        CreateButtonBehaviour();
        generalMenu.Open();
    }

    private void CreateButtonBehaviour()
    {
        boundButtons = new List<(PassiveButton, IBaseOptionMenuComponent)>
        {
            (generalButton, generalMenu), (lotusButton, lotusMenu), (graphicButton, graphicsMenu), (ventLibButton, ventLibMenu), (addonsButton, addonsMenu), (innerslothButton, innerslothMenu)
        };

        UnityAction ActionFunc(int i) =>
            (Action)(() =>
            {
                selectedButton = boundButtons[i].Item1;
                boundButtons.ForEach((b, i1) =>
                {
                    b.Item1.OnMouseOut.Invoke();
                    if (i == i1) b.Item2.Open();
                    else b.Item2.Close();
                });
                SoundManager.Instance.PlaySound(selectedButton.ClickSound, false);
            });

        generalButton.OnClick.AddListener(ActionFunc(0));
        lotusButton.OnClick.AddListener(ActionFunc(1));
        graphicButton.OnClick.AddListener(ActionFunc(2));
        ventLibButton.OnClick.AddListener(ActionFunc(3));
        addonsButton.OnClick.AddListener(ActionFunc(4));
        innerslothButton.OnClick.AddListener(ActionFunc(5));
    }



    // private Func<Sprite, string, PassiveButton> CreateButton(OptionsMenuBehaviour menuBehaviour)
    // {
    //     return (sprite, text) =>
    //     {

    //         PassiveButton button = Instantiate(template ??= menuBehaviour.GetComponentsInChildren<PassiveButton>().Last(), transform);
    //         TextMeshPro tmp = button.GetComponentInChildren<TextMeshPro>();
    //         SpriteRenderer render = button.GetComponentInChildren<SpriteRenderer>();
    //         render.sprite = sprite;
    //         render.color = Color.white;

    //         button.OnClick = new Button.ButtonClickedEvent();

    //         var buttonTransform = button.transform;
    //         buttonTransform.localScale -= new Vector3(0.33f, 0f, 0f);
    //         buttonTransform.localPosition += new Vector3(-4.6f, 2.5f, 0f);

    //         /*GameObject generalText = button.gameObject.CreateChild($"{text}_TextTMP", new Vector3(9.6f, -2.34f));
    //         tmp = generalText.AddComponent<TextMeshPro>();*/
    //         tmp.font = GetGeneralFont();
    //         tmp.fontSize = 2.8f;
    //         tmp.text = text;
    //         tmp.color = Color.white;
    //         tmp.transform.localPosition += new Vector3(0.13f, 0f);

    //         return button;
    //     };
    // }
    private delegate PassiveButton ButtonCreator(Sprite inactiveSprite, Sprite activeSprite, string text);

    private ButtonCreator CreateButton(OptionsMenuBehaviour menuBehaviour)
    {
        return (inactiveSprite, activeSprite, text) =>
        {
            PassiveButton button = Instantiate(template ??= menuBehaviour.GetComponentsInChildren<PassiveButton>().Last(), transform);
            TextMeshPro tmp = button.GetComponentInChildren<TextMeshPro>();
            SpriteRenderer render = button.GetComponentInChildren<SpriteRenderer>();
            render.sprite = inactiveSprite;
            render.color = Color.white;
            button.activeSprites = null;
            // button.name = text.ToLowerInvariant().Replace(" ", "");

            button.OnMouseOver = new UnityEngine.Events.UnityEvent();
            button.OnMouseOut = new UnityEngine.Events.UnityEvent();
            button.OnMouseOver.AddListener((Action)(() => render.sprite = activeSprite));
            button.OnMouseOut.AddListener((Action)(() => render.sprite = button == selectedButton ? activeSprite : inactiveSprite));

            button.OnClick = new Button.ButtonClickedEvent();

            var buttonTransform = button.transform;
            buttonTransform.localScale = new Vector3(0.765f, 1.417f, 1f);

            tmp.font = GetGeneralFont();
            tmp.fontSize = 2.8f;
            tmp.text = text;
            tmp.color = Color.white;
            // tmp.transform.localPosition += new Vector3(0.13f, 0f);
            tmp.transform.localPosition = new Vector3(.4f, 0f, 0f);
            tmp.transform.localScale = new Vector3(1.3f, .8f, 1f);

            return button;
        };
    }


    public static TMP_FontAsset GetGeneralFont()
    {
        return CustomOptionFont.OrElseSet(() =>
        {
            var embeddedFont = AssetLoader.LoadFont("Lotus.assets.Fonts.NunitoMedium.ttf");
            if (embeddedFont != null) return embeddedFont;
            DevLogger.Log("couldn't use embedded font. using a random system font.");
            string? path = Font.GetPathsToOSFonts()
                .FirstOrOptional(f => f.Contains("ARLRDBD"))
                .OrElseGet(() =>
                    Font.GetPathsToOSFonts().FirstOrOptional(f => f.Contains("ARIAL"))
                        .OrElseGet(() => Font.GetPathsToOSFonts().Count > 0 ? Font.GetPathsToOSFonts()[0] : null)
                );

            return path == null ? Resources.LoadAll("Fonts & Materials").ToArray().Select(t => t.TryCast<TMP_FontAsset>()).Last(t => t != null)! : TMP_FontAsset.CreateFontAsset(new Font(path));
        });
    }
}