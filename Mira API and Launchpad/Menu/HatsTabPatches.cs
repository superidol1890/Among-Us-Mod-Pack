using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Menu;

[HarmonyPatch(typeof(HatsTab))]
public static class HatsTabPatches
{
    private static SortedList<string, List<HatData>> sortedHats = [];
    private static int currentPage;

    [HarmonyPatch(nameof(HatsTab.OnEnable))]
    [HarmonyPrefix]
    public static bool OnEnablePrefix(HatsTab __instance)
    {
        __instance.currentHat = HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat);
        var allHats = HatManager.Instance.GetUnlockedHats().ToImmutableList();

        if (sortedHats.Count == 0)
        {
            var comparer = new ControllableComparer<string>(["vanilla"], [], StringComparer.InvariantCulture);
            sortedHats = new SortedList<string, List<HatData>>(comparer);
            foreach (var hat in allHats)
            {
                if (!sortedHats.ContainsKey(hat.StoreName)) sortedHats[hat.StoreName] = [];
                sortedHats[hat.StoreName].Add(hat);
            }
        }

        GenerateHats(__instance, currentPage);

        return false;
    }

    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
    [HarmonyPrefix]

    public static void UpdatePrefix(HatsTab __instance)
    {
        if (sortedHats.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentPage--;
            currentPage = currentPage < 0 ? sortedHats.Count - 1 : currentPage;
            GenerateHats(__instance, currentPage);
        }
        else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentPage++;
            currentPage = currentPage > sortedHats.Count - 1 ? 0 : currentPage;
            GenerateHats(__instance, currentPage);
        }
    }

    private static IEnumerator? loadRoutine;
    private static int hatIndex;

    private static void GenerateHats(HatsTab __instance, int page)
    {
        if (loadRoutine != null) Coroutines.Stop(loadRoutine);

        hatIndex = 0;
        foreach (var instanceColorChip in __instance.ColorChips) instanceColorChip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        __instance.scroller.Inner.GetComponentsInChildren<TextMeshPro>().Do(x => x.gameObject.Destroy());

        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);

        var (groupName, hats) = sortedHats.ToArray()[page];
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
        var xLerp = __instance.XRange.Lerp(0.5f);
        var yLerp = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
        text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);

        hatIndex += 5;
        loadRoutine = Coroutines.Start(CoGenerateChips(__instance, hats));
    }

    private static IEnumerator CoGenerateChips(HatsTab __instance, List<HatData> hats)
    {
        __instance.scroller.ScrollToTop();
        var batchSize = 5;
        for (var i = 0; i < hats.Count; i += batchSize)
        {
            var batch = hats.Skip(i).Take(batchSize).ToList();

            foreach (var hat in batch.OrderBy(HatManager.Instance.allHats.IndexOf))
            {
                var hatXposition = __instance.XRange.Lerp(hatIndex % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                var hatYposition = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
                GenerateColorChip(__instance, new Vector2(hatXposition, hatYposition), hat);
                hatIndex += 1;
                yield return null;
            }

            __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIndex + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
            yield return new WaitForSeconds(0.01f);
        }
        __instance.currentHatIsEquipped = true;
        loadRoutine = null;
    }

    private static void GenerateColorChip(HatsTab __instance, Vector2 position, HatData hat)
    {
        var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
        colorChip.gameObject.name = hat.ProductId;
        colorChip.Button.OnClick.AddListener((Action)(() => __instance.ClickEquip()));
        colorChip.Button.OnMouseOver.AddListener((Action)(() => __instance.SelectHat(hat)));
        colorChip.Button.OnMouseOut.AddListener((Action)(() => __instance.SelectHat(HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat))));
        colorChip.Inner.SetHat(hat, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
        colorChip.Button.ClickMask = __instance.scroller.Hitbox;
        colorChip.SelectionHighlight.gameObject.SetActive(false);
        __instance.UpdateMaterials(colorChip.Inner.FrontLayer, hat);
        colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
        colorChip.transform.localPosition = new Vector3(position.x, position.y, -1f);
        colorChip.Inner.transform.localPosition = hat.ChipOffset + new Vector2(0f, -0.3f);
        colorChip.Tag = hat;
        __instance.ColorChips.Add(colorChip);
    }
}
