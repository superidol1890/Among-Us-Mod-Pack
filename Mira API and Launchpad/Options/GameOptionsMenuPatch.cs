using System;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

/// <summary>
/// Patches the GameOptionsMenu to add custom options.
/// </summary>
[HarmonyPatch(typeof(GameOptionsMenu))]
public static class GameOptionsMenuPatch
{
    /// <summary>
    /// Update patch for the GameOptionsMenu.
    /// </summary>
    /// <param name="__instance">The GameOptionsMenu instance.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameOptionsMenu.Update))]
    public static void UpdatePatch(GameOptionsMenu __instance)
    {
        if (GameSettingMenuPatches.SelectedModIdx == 0)
        {
            return;
        }

        var num = 2.1f;
        if (__instance.name == "MODIFIERS TAB")
        {
            ModifiersUpdate(ref num);
        }
        else
        {
            var filteredGroups =
                GameSettingMenuPatches.SelectedMod?.OptionGroups
                    .Where(x => x.OptionableType == null && !x.ShowInModifiersMenu) ?? [];

            foreach (var group in filteredGroups)
            {
                UpdateGroup(group, ref num);
            }
        }

        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);
    }

    private static void UpdateGroup(AbstractOptionGroup group, ref float num)
    {
        if (group.Options.Count == 0 || group.Header is null)
        {
            return;
        }

        if (!group.GroupVisible.Invoke())
        {
            group.Header.gameObject.SetActive(false);
            foreach (var option in group.Options)
            {
                option.OptionBehaviour?.gameObject.SetActive(false);
            }

            return;
        }

        group.Header.gameObject.SetActive(true);
        group.Header.transform.localScale = Vector3.one * 0.63f;
        group.Header.transform.localPosition = new Vector3(-0.903f, num, -2f);

        num -= 0.58f;

        foreach (var opt in group.Options)
        {
            var newOpt = opt.OptionBehaviour;

            if (newOpt is null)
            {
                continue;
            }

            if (!opt.Visible.Invoke())
            {
                newOpt.gameObject.SetActive(false);
                continue;
            }

            if (!group.AllOptionsHidden)
            {
                newOpt.gameObject.SetActive(true);
                newOpt.transform.localPosition = new Vector3(0.952f, num, -2f);
                num -= 0.45f;
            }
            else
            {
                newOpt.gameObject.SetActive(false);
            }
        }
    }

    private static void ModifiersUpdate(ref float num)
    {
        var groups = GameSettingMenuPatches.SelectedMod?.OptionGroups
            .Where(x => x.ShowInModifiersMenu || x.OptionableType?.IsAssignableTo(typeof(BaseModifier))==true) ?? [];

        foreach (var modGroup in groups)
        {
            UpdateGroup(modGroup, ref num);
        }
    }

    private static void ModifiersCreate(GameOptionsMenu menu)
    {
        var groups = GameSettingMenuPatches.SelectedMod?.OptionGroups
            .Where(x => x.ShowInModifiersMenu || x.OptionableType?.IsAssignableTo(typeof(BaseModifier))==true) ?? [];
        foreach (var group in groups)
        {
            CreateGroup(menu, group);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings))]
    public static bool SettingsPatch(GameOptionsMenu __instance)
    {
        if (GameSettingMenuPatches.SelectedModIdx == 0)
        {
            return true;
        }

        __instance.MapPicker.gameObject.SetActive(false);

        if (__instance.name == "MODIFIERS TAB")
        {
            ModifiersCreate(__instance);
            return false;
        }

        var filteredGroups = GameSettingMenuPatches.SelectedMod?.OptionGroups
            .Where(x => x.OptionableType == null && !x.ShowInModifiersMenu) ?? [];

        foreach (var group in filteredGroups)
        {
            CreateGroup(__instance, group);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameOptionsMenu.Initialize))]
    public static bool InitPatch(GameOptionsMenu __instance)
    {
        if (__instance.Children != null && __instance.Children.Count != 0)
        {
            return false;
        }

        __instance.MapPicker.gameObject.SetActive(true);
        __instance.MapPicker.Initialize(20);
        var mapNameSetting = GameManager.Instance.GameSettingsList.MapNameSetting;
        __instance.MapPicker.SetUpFromData(mapNameSetting, 20);
        __instance.Children = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
        __instance.Children.Add(__instance.MapPicker);
        __instance.CreateSettings();
        __instance.cachedData = GameOptionsManager.Instance.CurrentGameOptions;
        foreach (var optionBehaviour in __instance.Children)
        {
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                optionBehaviour.SetAsPlayer();
            }

            if (optionBehaviour.IsCustom())
            {
                continue;
            }

            optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
        }

        return false;
    }

    private static void CreateGroup(GameOptionsMenu menu, AbstractOptionGroup group)
    {
        var categoryHeaderMasked = Object.Instantiate(
            menu.categoryHeaderOrigin,
            Vector3.zero,
            Quaternion.identity,
            menu.settingsContainer);

        categoryHeaderMasked.SetHeader(CustomStringName.CreateAndRegister(group.GroupName), 20);
        categoryHeaderMasked.Background.color = group.GroupColor;
        categoryHeaderMasked.Divider.color = group.GroupColor;
        categoryHeaderMasked.Title.color = group.GroupColor.Equals(MiraApiPlugin.DefaultHeaderColor) ? Color.white : group.GroupColor.FindAlternateColor();

        categoryHeaderMasked.Background.sprite = MiraAssets.CategoryHeader.LoadAsset();
        categoryHeaderMasked.Background.sprite.texture.filterMode = FilterMode.Bilinear;
        categoryHeaderMasked.Background.sprite.texture.wrapMode = TextureWrapMode.Clamp;

        categoryHeaderMasked.Background.transform.localPosition = new Vector3(0.5f, -0.1833f, 0);
        categoryHeaderMasked.Background.size = new Vector2(
            categoryHeaderMasked.Background.size.x + 1.5f,
            categoryHeaderMasked.Background.size.y);

        categoryHeaderMasked.gameObject.SetActive(false);
        group.Header = categoryHeaderMasked;

        var newText = Object.Instantiate(categoryHeaderMasked.Title, categoryHeaderMasked.transform);
        newText.text = "<size=70%>(Click to close)</size>";
        newText.transform.localPosition = new Vector3(2.6249f, -0.165f, 0f);
        newText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();

        var options = group.Options.Select(
            opt => opt.CreateOption(
                menu.checkboxOrigin,
                menu.numberOptionOrigin,
                menu.stringOptionOrigin,
                menu.settingsContainer));

        foreach (var newOpt in options)
        {
            newOpt.SetClickMask(menu.ButtonClickMask);

            SpriteRenderer[] componentsInChildren = newOpt.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in componentsInChildren)
            {
                if (group.GroupColor != MiraApiPlugin.DefaultHeaderColor)
                {
                    renderer.color = group.GroupColor.FindAlternateColor();
                    if (renderer.transform.parent.TryGetComponent<GameOptionButton>(out var btn))
                    {
                        btn.interactableColor = group.GroupColor.FindAlternateColor();
                        btn.interactableHoveredColor = Color.white;
                    }
                }

                renderer.material.SetInt(PlayerMaterial.MaskLayer, 20);
            }

            foreach (var textMeshPro in newOpt.GetComponentsInChildren<TextMeshPro>(true))
            {
                if (group.GroupColor != MiraApiPlugin.DefaultHeaderColor)
                {
                    textMeshPro.color = group.GroupColor;
                }

                textMeshPro.fontMaterial.SetFloat(ShaderID.StencilComp, 3f);
                textMeshPro.fontMaterial.SetFloat(ShaderID.Stencil, 20);
            }

            if (newOpt is ToggleOption toggle)
            {
                toggle.CheckMark.sprite = MiraAssets.Checkmark.LoadAsset();
                toggle.CheckMark.color = group.GroupColor != MiraApiPlugin.DefaultHeaderColor ? group.GroupColor : MiraAssets.AcceptedTeal;
                var rend = toggle.CheckMark.transform.parent.FindChild("ActiveSprite")
                    .GetComponent<SpriteRenderer>();
                rend.sprite = MiraAssets.CheckmarkBox.LoadAsset();
                rend.color = group.GroupColor != MiraApiPlugin.DefaultHeaderColor ? group.GroupColor : MiraAssets.AcceptedTeal;
            }

            menu.Children.Add(newOpt);

            newOpt.Initialize();
            newOpt.gameObject.SetActive(false);
        }

        var boxCol = categoryHeaderMasked.gameObject.AddComponent<BoxCollider2D>();
        boxCol.size = new Vector2(7, 0.7f);
        boxCol.offset = new Vector2(1.5f, -0.3f);

        var headerBtn = categoryHeaderMasked.gameObject.AddComponent<PassiveButton>();
        headerBtn.ClickSound = menu.BackButton.GetComponent<PassiveButton>().ClickSound;
        headerBtn.OnMouseOver = new UnityEvent();
        headerBtn.OnMouseOut = new UnityEvent();
        headerBtn.OnClick.AddListener(
            (UnityAction)(() =>
            {
                group.AllOptionsHidden = !group.AllOptionsHidden;
                newText.text = group.AllOptionsHidden
                    ? "<size=70%>(Click to open)</size>"
                    : "<size=70%>(Click to close)</size>";
            }));
        headerBtn.SetButtonEnableState(true);
    }
}
