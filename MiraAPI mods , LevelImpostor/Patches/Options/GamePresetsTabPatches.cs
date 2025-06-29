using System.Diagnostics;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Presets;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

internal static class GamePresetsTabPatches
{
    private static GameObject _saveButton = null!;
    private static GameObject _refreshButton = null!;
    private static GameObject _folderButton = null!;
    private static GameObject _newDivider = null!;
    private static GameObject _presetHolder = null!;
    private static GridArrange _arrange = null!;

    private static readonly object Lock = new();

    public static void DivideSize(GameObject obj, float amount)
    {
        foreach (var collider in obj.GetComponentsInChildren<Collider2D>(true))
        {
            if (collider.TryCast<BoxCollider2D>() is { } col)
            {
                col.size = new Vector2(col.size.x / amount, col.size.y);
            }
        }

        foreach (var rend in obj.GetComponentsInChildren<SpriteRenderer>(true))
        {
            rend.size = new Vector2(rend.size.x / amount, rend.size.y);
        }
    }

    [HarmonyPatch(typeof(GamePresetsTab), nameof(GamePresetsTab.OnEnable))]
    public static class GamePresetsOnEnablePatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(GamePresetsTab __instance)
        {
            if (!GameSettingMenu.Instance)
            {
                return;
            }

            CreatePresetMenu(__instance);

            __instance.StandardPresetButton.gameObject.SetActive(GameSettingMenuPatches.SelectedModIdx == 0);
            __instance.SecondPresetButton.gameObject.SetActive(GameSettingMenuPatches.SelectedModIdx == 0);

            if (GameSettingMenuPatches.SelectedModIdx != 0)
            {
                __instance.PresetDescriptionText.text = "Select or create an options preset for this mod";
            }
            else
            {
                __instance.SetSelectedText();
            }

            // Show the save and refresh buttons
            if (_saveButton)
            {
                _saveButton.SetActive(GameSettingMenuPatches.SelectedModIdx != 0);
            }
            if (_refreshButton)
            {
                _refreshButton.SetActive(GameSettingMenuPatches.SelectedModIdx != 0);
            }
            if (_folderButton)
            {
                _folderButton.SetActive(GameSettingMenuPatches.SelectedModIdx != 0);
            }
            if (_newDivider)
            {
                _newDivider.SetActive(GameSettingMenuPatches.SelectedModIdx != 0);
            }

            var prefab = GameSettingMenu.Instance.GameSettingsButton;

            foreach (var preset in MiraPluginManager.Instance.RegisteredPlugins.SelectMany(x => x.InternalPresets))
            {
                var button = Object.Instantiate(prefab, _presetHolder.transform);
                button.buttonText.text = preset.Name;
                if (button.buttonText.TryGetComponent<TextTranslatorTMP>(out var tmp))
                {
                    tmp.DestroyImmediate();
                }

                button.buttonText.alignment = TextAlignmentOptions.Center;

                button.OnClick = new Button.ButtonClickedEvent();
                button.OnClick.AddListener(
                    (UnityAction)(() =>
                    {
                        preset.LoadPreset();
                    }));
                button.gameObject.SetActive(false);
                preset.PresetButton = button.gameObject;
            }

            foreach (var mod in MiraPluginManager.Instance.RegisteredPlugins)
            {
                foreach (var button in mod.InternalPresets.Select(x => x.PresetButton))
                {
                    if (button != null)
                    {
                        button.SetActive(mod == GameSettingMenuPatches.SelectedMod);
                    }
                }
            }

            _arrange.Start();
            _arrange.ArrangeChilds();
        }
    }

    [HarmonyPatch(typeof(GamePresetsTab), nameof(GamePresetsTab.OnDisable))]
    public static class GamePresetsOnDisablePatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix()
        {
            Logger<MiraApiPlugin>.Error("OnDisable called");

            if (_saveButton)
            {
                _saveButton.SetActive(false);
            }
            if (_refreshButton)
            {
                _refreshButton.SetActive(false);
            }
            if (_folderButton)
            {
                _folderButton.SetActive(false);
            }

            if (_newDivider)
            {
                _newDivider.SetActive(false);
            }

            foreach (var mod in MiraPluginManager.Instance.RegisteredPlugins)
            {
                foreach (var button in mod.InternalPresets.Select(x => x.PresetButton))
                {
                    if (button != null)
                    {
                        button.SetActive(false);
                    }
                }
            }
        }
    }

    public static void CreatePresetMenu(GamePresetsTab presetTab)
    {
        var prefab = GameSettingMenu.Instance.GameSettingsButton;
        if (prefab == null)
        {
            Logger<MiraApiPlugin>.Error("GameSettingsButton prefab is null");
            return;
        }

        var tmpTranslator = presetTab.PresetDescriptionText.gameObject.GetComponent<TextTranslatorTMP>();
        if (tmpTranslator)
        {
            tmpTranslator.Destroy();
        }

        if (!_newDivider)
        {
            var oldDiv = presetTab.transform.FindChild("DividerImage");
            _newDivider = Object.Instantiate(oldDiv.gameObject, oldDiv.transform.parent);
            _newDivider.transform.localPosition = new Vector3(1.85f, 0.13f, 0f);
            _newDivider.transform.localScale = new Vector3(1.13f, 1, 1);
            _newDivider.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }

        PassiveButton saveButton;
        if (!_saveButton)
        {
            saveButton = Object.Instantiate(prefab, presetTab.transform);
            _saveButton = saveButton.gameObject;
            _saveButton.gameObject.name = "SaveButton";
            _saveButton.transform.localPosition = new Vector3(3.2f, 1.7f, -2);

            // set the button text and alignment
            var saveText = saveButton.buttonText;
            saveText.text = "Save";
            saveText.GetComponent<TextTranslatorTMP>().Destroy();
            saveText.alignment = TextAlignmentOptions.Center;
            saveText.transform.parent.localPosition = new Vector3(
                -.525f,
                saveText.transform.parent.localPosition.y,
                saveText.transform.parent.localPosition.z);

            DivideSize(saveButton.gameObject, 2f);

            saveButton.OnClick = new Button.ButtonClickedEvent();
            saveButton.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    if (GameSettingMenuPatches.SelectedModIdx == 0)
                    {
                        return;
                    }

                    SavePresetPopup.CreatePopup(
                        name =>
                        {
                            if (GameSettingMenuPatches.SelectedMod == null)
                            {
                                return;
                            }

                            if (name == string.Empty)
                            {
                                return;
                            }

                            var presetFile = new ConfigFile(
                                Path.Combine(
                                    PresetManager.PresetDirectory,
                                    GameSettingMenuPatches.SelectedMod.PluginId,
                                    $"{name}.cfg"),
                                false)
                            {
                                SaveOnConfigSet = false,
                            };

                            foreach (var option in GameSettingMenuPatches.SelectedMod.InternalOptions.Where(x => x.IncludeInPreset))
                            {
                                option.SaveToPreset(presetFile);
                            }

                            foreach (var role in GameSettingMenuPatches.SelectedMod.InternalRoles.Values.OfType<ICustomRole>().Where(x=>!x.Configuration.HideSettings))
                            {
                                role.SaveToPreset(presetFile);
                            }

                            presetFile.Save();
                            Refresh();
                        });
                }));
        }

        saveButton = _saveButton.GetComponent<PassiveButton>();

        PassiveButton refreshButton;
        if (!_refreshButton)
        {
            refreshButton = Object.Instantiate(saveButton, presetTab.transform);
            _refreshButton = refreshButton.gameObject;
            _refreshButton.name = "RefreshButton";
            _refreshButton.transform.localPosition = new Vector3(2.85f, 1.1f, -2);

            var icon = new GameObject("Sprite");
            icon.transform.SetParent(refreshButton.transform);
            icon.transform.localPosition = new Vector3(-0.47f, -0.08f, -5f);
            icon.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            icon.layer = refreshButton.gameObject.layer;

            var iconSpriteRend = icon.AddComponent<SpriteRenderer>();
            iconSpriteRend.sprite = MiraAssets.RefreshIcon.LoadAsset();

            DivideSize(refreshButton.gameObject, 2f);
            refreshButton.buttonText.gameObject.Destroy();

            refreshButton.OnClick = new Button.ButtonClickedEvent();
            refreshButton.OnClick.AddListener((UnityAction)Refresh);
        }
        refreshButton = _refreshButton.GetComponent<PassiveButton>();

        if (!_folderButton)
        {
            var openFolderButton = Object.Instantiate(refreshButton, presetTab.transform);
            _folderButton = openFolderButton.gameObject;
            _folderButton.name = "OpenFolderButton";
            _folderButton.transform.localPosition = new Vector3(3.55f, 1.1f, -2);

            var folderRend = openFolderButton.transform.FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>();
            folderRend.sprite = MiraAssets.FolderIcon.LoadAsset();
            folderRend.transform.localScale = new Vector3(0.4f, 0.4f, 1);

            openFolderButton.OnClick = new Button.ButtonClickedEvent();
            openFolderButton.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    var directory = Path.Combine(
                        PresetManager.PresetDirectory,
                        GameSettingMenuPatches.SelectedMod?.PluginId ?? string.Empty);
                    Directory.CreateDirectory(directory);
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = directory,
                            UseShellExecute = true,
                            Verb = "open",
                        });
                }));
        }

        _saveButton.SetActive(false);
        _refreshButton.SetActive(false);
        _folderButton.SetActive(false);
        _newDivider.SetActive(false);

        if (!_presetHolder)
        {
            _presetHolder = new GameObject("PresetHolder");
            _presetHolder.transform.SetParent(presetTab.transform, false);
            _presetHolder.transform.localScale = new Vector3(0.9f, 0.9f, 1);
            _presetHolder.transform.localPosition = new Vector3(-1.9f, 1.7f, 0);
            _arrange = _presetHolder.AddComponent<GridArrange>();
            _arrange.Alignment = GridArrange.StartAlign.Right;
            _arrange.CellSize = new Vector2(2.15f, -.55f);
            _arrange.MaxColumns = 2;
        }
    }

    private static void Refresh()
    {
        Logger<MiraApiPlugin>.Error("Refreshing presets");
        if (GameSettingMenuPatches.SelectedMod == null)
        {
            Logger<MiraApiPlugin>.Error("Selected mod is null, cannot refresh presets");
            return;
        }
        lock (Lock)
        {
            PresetManager.LoadPresets(GameSettingMenuPatches.SelectedMod);
        }
        GameSettingMenu.Instance.ChangeTab(0, false); // refresh the tab
    }
}
