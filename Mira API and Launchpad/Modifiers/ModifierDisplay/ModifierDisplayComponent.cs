using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Modifiers.ModifierDisplay;

/// <summary>
/// The code used to display Mira modifiers.
/// </summary>
[RegisterInIl2Cpp]
public class ModifierDisplayComponent(nint ptr) : MonoBehaviour(ptr)
{
    /// <summary>
    /// Gets the instance of the Modifier Display.
    /// </summary>
    public static ModifierDisplayComponent? Instance { get; private set; }

    private RectTransform _children = null!;
    private GameObject _modTemplate = null!;
    private PassiveButton _toggleButton = null!;
    private TextMeshPro _toggleBtnText = null!;

    private RectTransform _pagination = null!;
    private TextMeshPro _pageText = null!;
    private PassiveButton _nextButton = null!;
    private PassiveButton _backButton = null!;

    private int _currentPage = 0;
    private const int _itemsPerPage = 3;

    private readonly Dictionary<BaseModifier, ModifierUiComponent> _modifiers = [];

    /// <summary>
    /// Gets a read only dictionary of the created modifier UI components.
    /// </summary>
    [HideFromIl2Cpp]
    public ReadOnlyDictionary<BaseModifier, ModifierUiComponent> Modifiers => new(_modifiers);

    /// <summary>
    /// Gets a value indicating whether the modifier display hud is currently open.
    /// </summary>
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        Instance = this;

        _children = transform.GetChild(0).GetComponent<RectTransform>();
        _modTemplate = transform.GetChild(2).gameObject;

        _toggleButton = transform.GetChild(1).GetComponent<PassiveButton>();
        _toggleButton.OnClick = new Button.ButtonClickedEvent();
        _toggleButton.OnClick.AddListener((UnityAction)(() =>
        {
            IsOpen = !IsOpen;
            _children.gameObject.SetActive(!_children.gameObject.activeSelf);
        }));

        _toggleButton.ClickSound = HudManager.Instance.GameMenu.StreamerModeButton.GetComponent<PassiveButton>().ClickSound;
        _toggleButton.HoverSound = HudManager.Instance.GameMenu.StreamerModeButton.GetComponent<PassiveButton>().HoverSound;
        _toggleButton.inactiveSprites = _toggleButton.transform.GetChild(1).gameObject;
        _toggleBtnText = _toggleButton.transform.GetChild(0).GetComponent<TextMeshPro>();

        _pagination = _children.transform.GetChild(0).GetComponent<RectTransform>();
        _pageText = _pagination.transform.GetChild(0).GetComponent<TextMeshPro>();
        _nextButton = _pagination.transform.GetChild(1).GetComponent<PassiveButton>();
        _backButton = _pagination.transform.GetChild(2).GetComponent<PassiveButton>();

        _toggleBtnText.font = _pageText.font = _nextButton.buttonText.font = _backButton.buttonText.font = HudManager.Instance.TaskPanel.taskText.font;
        _toggleBtnText.fontMaterial = _pageText.fontMaterial = _nextButton.buttonText.fontMaterial = _backButton.buttonText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;

        _nextButton.OnClick = new Button.ButtonClickedEvent();
        _nextButton.OnClick.AddListener((UnityAction)(() =>
        {
            _currentPage++;
            UpdatePage();
        }));

        _backButton.OnClick = new Button.ButtonClickedEvent();
        _backButton.OnClick.AddListener((UnityAction)(() =>
        {
            _currentPage--;
            UpdatePage();
        }));

        IsOpen = false;
        _children.gameObject.SetActive(false);
    }

    /// <summary>
    /// Use this to refresh the modifiers UI. Useful if you hide modifiers uis based on options.
    /// </summary>
    public void RefreshModifiers()
    {
        UpdateModifiersList([.. PlayerControl.LocalPlayer.GetModifierComponent().ActiveModifiers]);
    }

    internal void DestroyComponent(ModifierUiComponent component)
    {
        _modifiers.Remove(component.Modifier!);
        component.gameObject.DestroyImmediate();

        RefreshModifiers();
    }

    [HideFromIl2Cpp]
    internal static ModifierDisplayComponent CreateDisplay()
    {
        var gameObject = Instantiate(MiraAssets.ModifierDisplay.LoadAsset(), HudManager.Instance.transform);
        var minigame = gameObject.AddComponent<ModifierDisplayComponent>();
        return minigame;
    }

    [HideFromIl2Cpp]
    private ModifierUiComponent CreateForModifier(BaseModifier modifier)
    {
        var newMod = Instantiate(_modTemplate, _children.transform);
        newMod.transform.name = modifier.ModifierName;
        var comp = newMod.AddComponent<ModifierUiComponent>();
        comp.Modifier = modifier;
        return comp;
    }

    private void UpdatePage()
    {
        var totalPages = Mathf.CeilToInt((float)_modifiers.Count / _itemsPerPage);
        _currentPage = Mathf.Clamp(_currentPage, 0, Mathf.Max(0, totalPages - 1));

        if (_modifiers.Count <= _itemsPerPage)
        {
            _modifiers.Do(x => x.Value.gameObject.SetActive(true));
            _pagination.gameObject.SetActive(false);
            return;
        }

        _pagination.gameObject.SetActive(true);
        if (_pageText != null)
        {
            _pageText.text = $"Page {_currentPage + 1}/{Mathf.Max(totalPages, 1)}";
        }

        _nextButton.gameObject.SetActive(true);
        _backButton.gameObject.SetActive(true);

        _modifiers.Do(x => x.Value.gameObject.SetActive(false));
        var start = _currentPage * _itemsPerPage;
        var end = Mathf.Min(start + _itemsPerPage, _modifiers.Count);
        var modifiers = _modifiers.ToArray();

        for (var i = start; i < end; i++)
        {
            modifiers[i].Value.gameObject.SetActive(true);
        }

        _pagination.SetAsLastSibling();
    }

    [HideFromIl2Cpp]
    internal void UpdateModifiersList(List<BaseModifier> modifiers)
    {
        var filteredModifiers = modifiers.Where(x => !x.HideOnUi && x.GetDescription() != string.Empty).ToList();
        var modifiersToAdd = filteredModifiers.ToList();

        foreach (var mod in _modifiers.ToArray())
        {
            if (modifiersToAdd.Contains(mod.Key))
            {
                modifiersToAdd.Remove(mod.Key);
                continue;
            }

            mod.Value.gameObject.Destroy();
            _modifiers.Remove(mod.Key);
        }

        _toggleBtnText.text = $"Modifiers ({filteredModifiers.Count})";

        if (filteredModifiers.Count == 0)
        {
            _toggleButton.gameObject.SetActive(false);
            return;
        }

        _toggleButton.gameObject.SetActive(true);

        foreach (var mod in modifiersToAdd)
        {
            _modifiers.Add(mod, CreateForModifier(mod));
        }

        UpdatePage();
    }
}
