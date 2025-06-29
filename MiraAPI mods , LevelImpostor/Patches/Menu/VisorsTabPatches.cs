using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Menu;

[HarmonyPatch(typeof(VisorsTab))]
public static class VisorsTabPatches
{
    private static readonly SortedList<string, List<VisorData>> SortedVisors = new(new ControllableComparer<string>(["vanilla"], [], StringComparer.InvariantCulture));
    private static int currentPage;
    internal static void AddRange(IEnumerable<(string Key, VisorData Visor)> data)
    {
        foreach (var item in data)
        {
            if (!SortedVisors.ContainsKey(item.Key)) SortedVisors.Add(item.Key, []);
            SortedVisors[item.Key].Add(item.Visor);
        }
    }

    [HarmonyPatch(nameof(VisorsTab.OnEnable))]
    [HarmonyPrefix]
    public static bool OnEnablePrefix(VisorsTab __instance)
    {
        __instance.visorId = HatManager.Instance.GetVisorById(DataManager.Player.Customization.Visor).ProdId;

        if (!SortedVisors.ContainsKey("Vanilla")) AddRange(DestroyableSingleton<HatManager>.Instance.GetUnlockedVisors().Select(x => ("Vanilla", x)));
        GenerateHats(__instance, currentPage);

        return false;
    }

    [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.Update))]
    [HarmonyPrefix]

    public static void UpdatePrefix(VisorsTab __instance)
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentPage--;
            currentPage = currentPage < 0 ? SortedVisors.Count - 1 : currentPage;
            GenerateHats(__instance, currentPage);
        }
        else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentPage++;
            currentPage = currentPage > SortedVisors.Count - 1 ? 0 : currentPage;
            GenerateHats(__instance, currentPage);
        }
    }

    private static void GenerateHats(VisorsTab __instance, int page)
    {
        foreach (ColorChip instanceColorChip in __instance.ColorChips) instanceColorChip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        __instance.scroller.Inner.GetComponentsInChildren<TextMeshPro>().Do(x => x.gameObject.Destroy());

        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);

        int hatIndex = 0;

        var (groupName, visors) = SortedVisors.ToArray()[page];
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
        foreach (var visor in visors.OrderBy(HatManager.Instance.allVisors.IndexOf))
        {
            float hatXposition = __instance.XRange.Lerp(hatIndex % __instance.NumPerRow / (__instance.NumPerRow - 1f));
            float hatYposition = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
            GenerateColorChip(__instance, new Vector2(hatXposition, hatYposition), visor);
            hatIndex += 1;
        }

        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIndex + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
        __instance.currentVisorIsEquipped = true;
    }

    private static void GenerateColorChip(VisorsTab __instance, Vector2 position, VisorData visor)
    {
        var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
        colorChip.gameObject.name = visor.ProductId;
        colorChip.Button.OnClick.AddListener((Action)(() => __instance.ClickEquip()));
        colorChip.Button.OnMouseOver.AddListener((Action)(() => __instance.SelectVisor(visor)));
        colorChip.Button.OnMouseOut.AddListener((Action)(() => __instance.SelectVisor(HatManager.Instance.GetVisorById(DataManager.Player.Customization.Visor))));
        colorChip.Button.ClickMask = __instance.scroller.Hitbox;
        colorChip.ProductId = visor.ProductId;
        colorChip.SelectionHighlight.gameObject.SetActive(false);
        __instance.UpdateMaterials(colorChip.Inner.FrontLayer, visor);
        visor.SetPreview(colorChip.Inner.FrontLayer, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
        colorChip.transform.localPosition = new Vector3(position.x, position.y, -1f);
        colorChip.Inner.transform.localPosition = visor.ChipOffset + new Vector2(0f, -0.3f);
        colorChip.Tag = visor;
        __instance.ColorChips.Add(colorChip);
    }
}
