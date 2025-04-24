using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace LaunchpadReloaded.API.Settings;

public static class CustomSettingsManager
{
    public static readonly List<CustomSetting> CustomSettings = [];

    public static void CreateTab(OptionsMenuBehaviour optionsMenu) 
    {
        // replace help tab (its unused in game iirc)

        var startX = HudManager.InstanceExists ? -1.65f : -2.45f;
        var yOffset = HudManager.InstanceExists ? .1f : 0;
        for (var i = 0; i < optionsMenu.Tabs.Count; i++)
        {
            var pos = optionsMenu.Tabs[i].transform.localPosition; 
            optionsMenu.Tabs[i].transform.localPosition = new Vector3(startX + i*1.65f, pos.y+yOffset, pos.z);
        }

        var newTabButton = optionsMenu.Tabs.Last();
        var newTabButtonText = newTabButton.GetComponentInChildren<TextMeshPro>();
        newTabButton.GetComponentInChildren<TextTranslatorTMP>().Destroy();
        newTabButton.name = "LaunchpadButton";
        newTabButtonText.text = "Launchpad";
        newTabButton.gameObject.SetActive(true);

        var launchpadTab = newTabButton.Content;
        launchpadTab.name = "LaunchpadTab";
        launchpadTab.transform.DestroyChildren();
        var gridArrange = launchpadTab.AddComponent<GridArrange>();
        var aspectPosition = launchpadTab.AddComponent<AspectPosition>();
        gridArrange.Alignment = GridArrange.StartAlign.Left;
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Top;
        gridArrange.CellSize = new Vector2(2.6f, -0.5f);
        gridArrange.MaxColumns = 2;
        
        foreach (var customOption in CustomSettings)
        {
            customOption.CreateButton(optionsMenu, launchpadTab.transform);
        }

        aspectPosition.updateAlways = true;
        aspectPosition.DistanceFromEdge = new Vector3(1.3f, 1f, 0);
        gridArrange.Start();
        gridArrange.ArrangeChilds();

        aspectPosition.AdjustPosition();
    }
}