using System;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[RegisterInIl2Cpp]
#pragma warning disable S3903
#pragma warning disable CA1050
public class SavePresetPopup(nint cppPtr) : Minigame(cppPtr)
#pragma warning restore CA1050
#pragma warning restore S3903
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private TextMeshPro textBoxText;
    private TextBoxTMP textBox;

    private Action<string> onSave;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    // Cleanup holder object
    public void OnDestroy()
    {
        transform.parent.gameObject.Destroy();
    }

    public override void Close()
    {
        // no-op
    }

    private void Awake()
    {
        var textboxHolder = transform.GetChild(1).GetChild(1);
        var saveButton = transform.FindChild("SaveButton").GetComponent<PassiveButton>();
        var closeButton = transform.FindChild("CloseButton").GetComponent<PassiveButton>();
        textBox = textboxHolder.GetChild(0).GetComponent<TextBoxTMP>();
        textBoxText = textBox.transform.GetChild(0).GetComponent<TextMeshPro>();

        saveButton.OnClick = new Button.ButtonClickedEvent();
        saveButton.OnClick.AddListener((UnityAction)(() =>
        {
            onSave.Invoke(textBoxText.text);
            this.BaseClose();
        }));

        closeButton.OnClick = new Button.ButtonClickedEvent();
        closeButton.OnClick.AddListener((UnityAction)(() =>
        {
            this.BaseClose();
        }));

        textBox.OnChange = new Button.ButtonClickedEvent();
        textBox.OnChange.AddListener((UnityAction)(() =>
        {
            textBoxText.text = textBoxText.text.Replace(" ", string.Empty);
        }));

        textBox.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            textBox.GiveFocus();
        }));

        foreach (var tmp in gameObject.GetComponentsInChildren<TextMeshPro>())
        {
            if (tmp.gameObject.transform.parent.name == "Textbox")
            {
                continue;
            }

            var text = tmp.text;
            tmp.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF RadialMenu Material\">{text}</font>";
        }
    }

    public static void CreatePopup(Action<string> saveAction)
    {
        // Because innerscuff doesnt account for Z value in Close animation
        var holder = new GameObject("PopupHolder")
        {
            transform =
            {
                parent = Camera.main!.transform,
                localPosition = new Vector3(0, -1f, -500),
            },
        };

        var gameObject = Instantiate(MiraAssets.PresetSavePopup.LoadAsset(), holder.transform);
        var popup = gameObject.GetComponent<SavePresetPopup>();
        popup.onSave = saveAction;
        popup.Begin(null);
    }
}



