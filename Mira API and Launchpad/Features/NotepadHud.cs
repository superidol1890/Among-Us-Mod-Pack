using LaunchpadReloaded.Options;
using MiraAPI.GameOptions;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace LaunchpadReloaded.Features;

public class NotepadHud
{
    public static NotepadHud? Instance { get; private set; }
    public GameObject NotepadButton { get; private set; } = null!;
    public GameObject Notepad { get; private set; } = null!;

    private readonly HudManager _hud;
    private AspectPosition _buttonAspectPos = null!;

    public NotepadHud(HudManager hud)
    {
        _hud = hud;
        Instance = this;

        CreateNotepad();
        CreateNotepadButton();
        UpdateAspectPos();
        SetNotepadButtonVisible(OptionGroupSingleton<GeneralOptions>.Instance.Notepad);
    }

    public void UpdateAspectPos()
    {
        _buttonAspectPos.DistanceFromEdge = MeetingHud.Instance || HudManager.Instance.Chat.chatButton.gameObject.active || PlayerControl.LocalPlayer.Data.IsDead
            ? new Vector3(2.75f, 0.505f, -400f) : new Vector3(2.15f, 0.505f, -400f);

        _buttonAspectPos.AdjustPosition();
    }
    public void Destroy()
    {
        Notepad.gameObject.DestroyImmediate();
        NotepadButton.gameObject.DestroyImmediate();

        Instance = null;
    }

    public void SetNotepadButtonVisible(bool visible)
    {
        NotepadButton.gameObject.SetActive(visible);
    }

    private void CreateNotepadButton()
    {
        NotepadButton = Object.Instantiate(_hud.SettingsButton.gameObject, _hud.SettingsButton.transform.parent);
        NotepadButton.name = "NotepadButton";

        _buttonAspectPos = NotepadButton.GetComponent<AspectPosition>();
        var activeSprite = NotepadButton.transform.FindChild("Active").GetComponent<SpriteRenderer>();
        var inactiveSprite = NotepadButton.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
        var textbox = Notepad.transform.FindChild("Textbox");
        var tmpTextBox = textbox.GetComponent<TextBoxTMP>();
        var passiveButton = NotepadButton.GetComponent<PassiveButton>();

        inactiveSprite.sprite = LaunchpadAssets.NotepadSprite.LoadAsset();
        activeSprite.sprite = LaunchpadAssets.NotepadActiveSprite.LoadAsset();

        activeSprite.transform.localPosition = inactiveSprite.transform.localPosition = new Vector3(0.005f, 0.025f, 0f);

        passiveButton.ClickSound = _hud.MapButton.ClickSound;
        passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            if (Minigame.Instance)
            {
                return;
            }

            Notepad.gameObject.SetActive(!Notepad.gameObject.active);

            if (Notepad.gameObject.active)
            {
                tmpTextBox.GiveFocus();
            }

            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }

            PlayerControl.LocalPlayer.MyPhysics.SetNormalizedVelocity(Vector2.zero);
        }));
    }

    private void CreateNotepad()
    {
        Notepad = Object.Instantiate(LaunchpadAssets.Notepad.LoadAsset(), HudManager.Instance.transform);
        Notepad.gameObject.SetActive(false);

        var textbox = Notepad.transform.FindChild("Textbox");
        var tmpTextBox = textbox.GetComponent<TextBoxTMP>();
        var psBtn = textbox.GetComponent<PassiveButton>();
        var closeButton = Notepad.transform.FindChild("CloseButton").GetComponent<PassiveButton>();

        closeButton.OnClick.AddListener((UnityAction)(() => Notepad.gameObject.SetActive(false)));

        psBtn.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        psBtn.OnClick.AddListener((UnityAction)(() =>
        {
            tmpTextBox.GiveFocus();
        }));

        closeButton.ClickSound = _hud.MapButton.ClickSound;
    }
}