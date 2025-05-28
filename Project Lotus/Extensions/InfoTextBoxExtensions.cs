using System;
using TMPro;
using UnityEngine;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.Extensions;

public static class InfoTextBoxExtensions
{
    public static (PassiveButton, TextMeshPro) SetThreeButtons(this InfoTextBox textBox)
    {
        textBox.SetTwoButtons();

        UnityOptional<PassiveButton> thirdButton = textBox.FindChildOrEmpty<PassiveButton>("Button3", true);
        if (!thirdButton.Exists()) thirdButton = UnityOptional<PassiveButton>.NonNull(Object.Instantiate(textBox.button1, textBox.button1Trans.parent));

        thirdButton.Get().name = "Button3";
        thirdButton.Get().gameObject.SetActive(true);
        thirdButton.Get().transform.localPosition = new Vector2(0f, thirdButton.Get().transform.localPosition.y);
        return (thirdButton.Get(), thirdButton.Get().FindChild<TextMeshPro>("Text_TMP", true));
    }

    [QuickPrefix(typeof(InfoTextBox), nameof(InfoTextBox.SetOneButton))]
    public static void SetOneButton(InfoTextBox __instance)
    {
        UnityOptional<PassiveButton> thirdButton = __instance.FindChildOrEmpty<PassiveButton>("Button3");
        thirdButton.IfPresent(b => b.gameObject.SetActive(false));
    }
    [QuickPrefix(typeof(InfoTextBox), nameof(InfoTextBox.SetTwoButtons))]
    public static void SetTwoButtons(InfoTextBox __instance)
    {
        UnityOptional<PassiveButton> thirdButton = __instance.FindChildOrEmpty<PassiveButton>("Button3");
        thirdButton.IfPresent(b => b.gameObject.SetActive(false));
    }
}