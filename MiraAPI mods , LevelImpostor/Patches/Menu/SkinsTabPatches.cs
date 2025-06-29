using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Menu;

[HarmonyPatch(typeof(SkinsTab))]
public static class SkinsTabPatches
{
    private static SortedList<string, List<SkinData>> sortedSkins = [];
    private static int currentPage;

    [HarmonyPatch(nameof(SkinsTab.OnEnable))]
    [HarmonyPrefix]
    public static bool OnEnablePrefix(SkinsTab __instance)
    {
        __instance.skinId = HatManager.Instance.GetSkinById(DataManager.Player.Customization.Skin).ProdId;
        var allSkins = HatManager.Instance.GetUnlockedSkins().ToImmutableList();

        if (sortedSkins.Count == 0)
        {
            var comparer = new ControllableComparer<string>(["vanilla"], [], StringComparer.InvariantCulture);
            sortedSkins = new SortedList<string, List<SkinData>>(comparer);
            foreach (var skin in allSkins)
            {
                if (!sortedSkins.ContainsKey(skin.StoreName)) sortedSkins[skin.StoreName] = [];
                sortedSkins[skin.StoreName].Add(skin);
            }
        }

        GenerateHats(__instance, currentPage);

        return false;
    }

    [HarmonyPatch(typeof(SkinsTab), nameof(SkinsTab.Update))]
    [HarmonyPrefix]

    public static void UpdatePrefix(SkinsTab __instance)
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentPage--;
            currentPage = currentPage < 0 ? sortedSkins.Count - 1 : currentPage;
            GenerateHats(__instance, currentPage);
        }
        else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentPage++;
            currentPage = currentPage > sortedSkins.Count - 1 ? 0 : currentPage;
            GenerateHats(__instance, currentPage);
        }
    }

    private static void GenerateHats(SkinsTab __instance, int page)
    {
        foreach (ColorChip instanceColorChip in __instance.ColorChips) instanceColorChip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        __instance.scroller.Inner.GetComponentsInChildren<TextMeshPro>().Do(x => x.gameObject.Destroy());

        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);

        int hatIndex = 0;

        var (groupName, skins) = sortedSkins.ToArray()[page];
        var text = Object.Instantiate(groupNameText, __instance.scroller.Inner);
        text.enabled = true;
        text.gameObject.transform.localScale = Vector3.one;
        text.GetComponent<TextTranslatorTMP>().Destroy();
        text.EnableStencilMasking();
        text.text = $"{groupName}\nPress Ctrl or Tab to cycle pages";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 3f;
        text.fontSizeMax = 3f;
        text.fontSizeMin = 0f;
        float xLerp = __instance.XRange.Lerp(0.5f);
        float yLerp = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
        text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);

        hatIndex += 5;
        foreach (var skin in skins.OrderBy(HatManager.Instance.allSkins.IndexOf))
        {
            float hatXposition = __instance.XRange.Lerp(hatIndex % __instance.NumPerRow / (__instance.NumPerRow - 1f));
            float hatYposition = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
            GenerateColorChip(__instance, new Vector2(hatXposition, hatYposition), skin);
            hatIndex += 1;
        }

        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIndex + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
        __instance.currentSkinIsEquipped = true;
    }

    private static void GenerateColorChip(SkinsTab __instance, Vector2 position, SkinData skin)
    {
        var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
        colorChip.gameObject.name = skin.ProductId;
        colorChip.Button.OnClick.AddListener((Action)(() => __instance.ClickEquip()));
        colorChip.Button.OnMouseOver.AddListener((Action)(() => __instance.SelectSkin(skin)));
        colorChip.Button.OnMouseOut.AddListener((Action)(() => __instance.SelectSkin(HatManager.Instance.GetSkinById(DataManager.Player.Customization.Skin))));
        colorChip.Button.ClickMask = __instance.scroller.Hitbox;
        colorChip.ProductId = skin.ProductId;
        colorChip.SelectionHighlight.gameObject.SetActive(false);
        __instance.UpdateMaterials(colorChip.Inner.FrontLayer, skin);
        skin.SetPreview(colorChip.Inner.FrontLayer, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
        colorChip.transform.localPosition = new Vector3(position.x, position.y, -1f);
        colorChip.Inner.transform.localPosition = skin.ChipOffset + new Vector2(0f, -0.3f);
        colorChip.Tag = skin;
        __instance.ColorChips.Add(colorChip);
    }
}
