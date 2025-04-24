using System;
using System.Linq;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameSettingMenu))]
internal static class GameSettingMenuPatches
{
    public static int SelectedModIdx { get; private set; }

    public static MiraPluginInfo? SelectedMod { get; private set; }

    private static TextMeshPro? _text;

    private static Vector3 _roleBtnOgPos;
    private static Vector3 _smallRoleBtnOgPos;
    private static Vector3 _modifierBtnOgPos;

    private static GameOptionsMenu? _modifiersTab;
    private static PassiveButton? _modifiersButton;
    private static PassiveButton? _smallRolesButton;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    public static void ChangeTabPostfix(GameSettingMenu __instance, int tabNum, bool previewOnly)
    {
        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            if (_modifiersTab) _modifiersTab!.gameObject.SetActive(tabNum == 3);
            _modifiersButton?.SelectButton(tabNum == 3);
            _smallRolesButton?.SelectButton(tabNum == 2);
            if (tabNum == 3)
            {
                if (__instance.RoleSettingsButton.gameObject.active)
                {
                    __instance.RoleSettingsButton.SelectButton(true);
                }
                else
                {
                    _modifiersButton?.SelectButton(true);
                }
            }
        }

        if (previewOnly)
        {
            return;
        }

        _modifiersButton?.SelectButton(tabNum == 3);
        _smallRolesButton?.SelectButton(tabNum == 2);

        if (tabNum != 3) return;

        if (__instance.RoleSettingsButton.gameObject.active)
        {
            __instance.RoleSettingsButton.SelectButton(true);
        }
        else
        {
            _modifiersButton?.SelectButton(true);
        }
    }

    /// <summary>
    /// Prefix for the <see cref="GameSettingMenu.Start"/> method. Sets up the custom options.
    /// </summary>
    /// <param name="__instance">The GameSettingMenu instance.</param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    public static void StartPrefix(GameSettingMenu __instance)
    {
        _roleBtnOgPos = __instance.RoleSettingsButton.transform.localPosition;
        __instance.transform.FindChild("GameSettingsLabel").gameObject.SetActive(false);

        var helpThing = __instance.transform.FindChild("What Is This?");
        var tmpText = Object.Instantiate(helpThing.transform.FindChild("InfoText"), helpThing.parent).gameObject;

        tmpText.GetComponent<TextTranslatorTMP>().Destroy();
        tmpText.name = "SelectedMod";
        tmpText.transform.localPosition = new Vector3(-3.3382f, 1.5399f, -2);

        _text = tmpText.GetComponent<TextMeshPro>();
        _text.fontSizeMax = 3.2f;
        _text.overflowMode = TextOverflowModes.Overflow;

        _text.alignment = TextAlignmentOptions.Center;

        var nextButton = Object.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.transform.localPosition = new Vector3(-2.2663f, 1.5272f, -25f);
        nextButton.name = "RightArrowButton";
        nextButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButton.LoadAsset();
        nextButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButtonActive.LoadAsset();
        nextButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();

        var passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                SelectedModIdx += 1;
                if (SelectedModIdx > MiraPluginManager.Instance.RegisteredPlugins().Length)
                {
                    SelectedModIdx = 0;
                }

                UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
            }));

        var backButton = Object.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-4.4209f, 1.5272f, -25f);
        backButton.name = "LeftArrowButton";
        backButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        backButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().flipX =
            backButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().flipX = true;
        backButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener(
            (UnityAction)(() =>
            {
                SelectedModIdx -= 1;
                if (SelectedModIdx < 0)
                {
                    SelectedModIdx = MiraPluginManager.Instance.RegisteredPlugins().Length;
                }

                UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
            }));

        // clone game settings tab for modifiers
        _modifiersTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        _modifiersTab.name = "MODIFIERS TAB";

        // create button for modifiers
        __instance.RoleSettingsButton.buttonText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        __instance.RoleSettingsButton.gameObject.SetActive(false);
        _smallRolesButton = Object.Instantiate(
            __instance.RoleSettingsButton,
            __instance.RoleSettingsButton.transform.parent);
        var pos = new Vector3(
            -3.65f,
            _smallRolesButton.transform.localPosition.y,
            _smallRolesButton.transform.localPosition.z);
        _smallRolesButton.transform.localPosition = pos;
        _smallRolesButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(2, false);
            }));
        _smallRolesButton.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(2, true);
            }));

        _smallRoleBtnOgPos = _smallRolesButton.transform.localPosition;

        var roleText = _smallRolesButton.buttonText;
        roleText.text = "Roles";
        roleText.GetComponent<TextTranslatorTMP>().Destroy();
        roleText.alignment = TextAlignmentOptions.Center;
        roleText.transform.parent.localPosition = new Vector3(
            -.525f,
            roleText.transform.parent.localPosition.y,
            roleText.transform.parent.localPosition.z);

        foreach (var collider in _smallRolesButton.Colliders)
        {
            if (collider.TryCast<BoxCollider2D>() is { } col)
            {
                col.size = new Vector2(col.size.x / 2, col.size.y);
            }
        }

        foreach (var rend in _smallRolesButton.GetComponentsInChildren<SpriteRenderer>(true))
        {
            rend.size = new Vector2(rend.size.x / 2, rend.size.y);
        }

        _modifiersButton = Object.Instantiate(_smallRolesButton, _smallRolesButton.transform.parent);
        _modifiersButton.OnClick = new ButtonClickedEvent();
        _modifiersButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(3, false);
            }));
        _modifiersButton.OnMouseOver = new UnityEvent();
        _modifiersButton.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(3, true);
            }));

        _modifiersButton.buttonText.text = "Modifiers";
        pos.x = -2.27f;
        _modifiersButton.transform.localPosition = pos;
        _modifiersButton.name = "ModifiersButton";

        _modifierBtnOgPos = _modifiersButton.transform.localPosition;

        UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
    }

    private static void ChangeRoleSettingButton(bool replace, GameSettingMenu __instance)
    {
        __instance.RoleSettingsButton.OnClick = new ButtonClickedEvent();
        __instance.RoleSettingsButton.OnMouseOver = new UnityEvent();

        if (replace)
        {
            __instance.RoleSettingsButton.buttonText.text = "Modifiers Settings";
            __instance.RoleSettingsButton.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    __instance.ChangeTab(3, false);
                }));
            __instance.RoleSettingsButton.OnMouseOver.AddListener(
                (UnityAction)(() =>
                {
                    __instance.ChangeTab(3, true);
                }));
        }
        else
        {
            __instance.RoleSettingsButton.buttonText.text = "Roles Settings";
            __instance.RoleSettingsButton.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    __instance.ChangeTab(2, false);
                }));
            __instance.RoleSettingsButton.OnMouseOver.AddListener(
                (UnityAction)(() =>
                {
                    __instance.ChangeTab(2, true);
                }));
        }
    }
    private static void UpdateText(GameSettingMenu menu, GameOptionsMenu settings, RolesSettingsMenu roles)
    {
        if (_text is not null && SelectedModIdx == 0)
        {
            _text.text = "Default";
            _text.fontSizeMax = 3.2f;
        }
        else if (_text is not null)
        {
            _text.fontSizeMax = 2.3f;
            SelectedMod = MiraPluginManager.Instance.RegisteredPlugins()[SelectedModIdx - 1];

            var name = SelectedMod.MiraPlugin.OptionsTitleText;
            _text.text = name[..Math.Min(name.Length, 25)];
        }

        bool replaceWithModifiers = true;
        _smallRolesButton!.transform.localPosition = _smallRoleBtnOgPos;
        _modifiersButton!.transform.localPosition = _modifierBtnOgPos;
        menu.RoleSettingsButton.transform.localPosition = _roleBtnOgPos;

        if (SelectedModIdx != 0)
        {
            var modHasRoles = SelectedMod!.CustomRoles.Count != 0;
            var modHasModifiers = SelectedMod.OptionGroups.Exists(x => x.ShowInModifiersMenu || x.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true);
            var modHasOptions = SelectedMod.OptionGroups.Exists(x => x.OptionableType == null && !x.ShowInModifiersMenu);

            _modifiersButton.gameObject.SetActive(true);
            _smallRolesButton.gameObject.SetActive(true);
            menu.GameSettingsButton.gameObject.SetActive(true);
            menu.RoleSettingsButton.gameObject.SetActive(false);

            // If there are no registered custom roles in the selected mod, hide the button.
            if (!modHasRoles)
            {
                _smallRolesButton.gameObject.SetActive(false);

                if (roles.gameObject.active)
                {
                    menu.ChangeTab(0, false);
                }

                // If the mod has modifiers, we can disable the smaller modifier button and enable the bigger one.
                if (modHasModifiers)
                {
                    _modifiersButton.gameObject.SetActive(false);
                    menu.RoleSettingsButton.gameObject.SetActive(true);

                    if (_modifiersTab!.gameObject.active)
                    {
                        menu.RoleSettingsButton.SelectButton(true);
                    }
                }
            }

            // If there are no modifiers in the selected mod, hide the modifiers button.
            if (!modHasModifiers)
            {
                replaceWithModifiers = false;
                _modifiersButton.gameObject.SetActive(false);

                if (_modifiersTab!.gameObject.active)
                {
                    menu.ChangeTab(0, false);
                }
                // If the mod has roles, we can enable the bigger role button and disable the small one.
                if (modHasRoles)
                {
                    menu.RoleSettingsButton.gameObject.SetActive(true);
                    _smallRolesButton.gameObject.SetActive(false);
                }
            }

            // If there are no custom game options registered (that aren't modifier options), hide game settings button.
            if (!modHasOptions)
            {
                menu.GameSettingsButton.gameObject.SetActive(false);

                if (settings.gameObject.active)
                {
                    menu.ChangeTab(0, false);
                }

                // If the mod has roles and modifiers, we can move their buttons to the game settings button position, since nothing is there.
                if (menu.RoleSettingsButton.gameObject.active)
                {
                    menu.RoleSettingsButton.transform.localPosition = menu.GameSettingsButton.transform.localPosition;
                }
                else if (_modifiersButton.gameObject.active && _smallRolesButton.gameObject.active)
                {
                    _modifiersButton.transform.localPosition = new Vector3(
                        _modifiersButton.transform.localPosition.x,
                        menu.GameSettingsButton.transform.localPosition.y,
                        _modifiersButton.transform.localPosition.z);

                    _smallRolesButton.transform.localPosition = new Vector3(
                        _smallRolesButton.transform.localPosition.x,
                        menu.GameSettingsButton.transform.localPosition.y,
                        _smallRolesButton.transform.localPosition.z);
                }
            }
        }
        else
        {
            _modifiersButton.gameObject.SetActive(false);
            _smallRolesButton.gameObject.SetActive(false);
            menu.RoleSettingsButton.gameObject.SetActive(true);
            menu.GameSettingsButton.gameObject.SetActive(true);
            replaceWithModifiers = false;

            if (_modifiersTab!.gameObject.active)
            {
                menu.ChangeTab(0, false);
            }
        }

        if (menu.RoleSettingsButton.gameObject.active)
        {
            ChangeRoleSettingButton(replaceWithModifiers, menu);
        }

        CleanupTab(settings, roles);
    }

    private static void ClearOptions(Il2CppSystem.Collections.Generic.List<OptionBehaviour> options)
    {
        foreach (var child in options)
        {
            if (child.TryCast<GameOptionsMapPicker>() || !child.gameObject)
            {
                continue;
            }
            child.gameObject.DestroyImmediate();
        }
        options.Clear();
    }

    private static void CleanupTab(GameOptionsMenu settings, RolesSettingsMenu roles)
    {
        void CleanupRoleSettings(RolesSettingsMenu rolesMenu)
        {
            if (rolesMenu.advancedSettingChildren != null)
            {
                ClearOptions(rolesMenu.advancedSettingChildren);
                rolesMenu.advancedSettingChildren = null;
            }

            rolesMenu.RoleChancesSettings
                .transform
                .GetComponentsInChildren<CategoryHeaderEditRole>()
                .ToList()
                .ForEach(header => header.gameObject.DestroyImmediate());

            foreach (var role in rolesMenu.roleChances)
            {
                role.gameObject.DestroyImmediate();
            }
            rolesMenu.roleChances?.Clear();

            rolesMenu.AdvancedRolesSettings.gameObject.SetActive(false);
            rolesMenu.RoleChancesSettings.gameObject.SetActive(true);
            rolesMenu.SetQuotaTab();
            rolesMenu.scrollBar.ScrollToTop();
        }

        void CleanupSettings(GameOptionsMenu gameOptMenu)
        {
            ClearOptions(gameOptMenu.Children);
            gameOptMenu.Children = null;

            gameOptMenu.settingsContainer
                .GetComponentsInChildren<CategoryHeaderMasked>()
                .ToList()
                .ForEach(header => header.gameObject.DestroyImmediate());

            gameOptMenu.Initialize();
            gameOptMenu.scrollBar.ScrollToTop();
        }

        if (roles.roleChances != null && SelectedMod?.CustomRoles.Count > 0)
        {
            CleanupRoleSettings(roles);
        }

        if (settings.Children != null && SelectedMod?.OptionGroups.Count > 0)
        {
            CleanupSettings(settings);
        }

        if (_modifiersTab?.Children?.Count > 0)
        {
            CleanupSettings(_modifiersTab);
        }
    }
}
