using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Lotus.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using Object = UnityEngine.Object;
using Lotus.Managers.Announcements.Models;
using Lotus.Managers;
using AmongUs.Data;
using VentLib.Utilities.Extensions;
using System.Globalization;
using Lotus;

namespace Lotus.GUI.Patches;

public class AnnouncementPatch
{
    public const string ModdedText = "IGNORE";
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(AnnouncementPatch));
    [QuickPostfix(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.CreateAnnouncementList))]
    public static void CreateAnnouncementsList(AnnouncementPopUp __instance)
    {
        PluginDataManager.AnnouncementManager.GetAnnouncements().ForEach(a =>
        {
            if (a.DevOnly && !ProjectLotus.DevVersion) return;
            AnnouncementPanel panel = Object.Instantiate<AnnouncementPanel>(__instance.AnnouncementPanelPrefab, __instance.AnnouncementListSlider.transform);
            panel.announcement = new Assets.InnerNet.Announcement() { Title = ModdedText, Number = 69 };
            panel.Background.enabled = false;
            panel.TitleText.text = a.ShortTitle;
            panel.DateText.text = a.FormattedToLanguage();
            panel.transform.localPosition = __instance.panelStartPos;
            panel.PassiveButton.ClickMask = __instance.ListScroller.Hitbox;
            __instance.visibleAnnouncements.Add(panel);
            ControllerManager.Instance.AddSelectableUiElement(panel.PassiveButton, false);
            if (PluginDataManager.AnnouncementManager.HasReadAnnouncement(a)) panel.MarkAsRead();
            panel.PassiveButton.OnClick.AddListener((Action)(() =>
            {
                __instance.selectedIndex = __instance.visibleAnnouncements.IndexOf(panel);
                AnnouncementPanel announcementPanel = __instance.selectedPanel;
                if (announcementPanel != null)
                {
                    announcementPanel.UnSelect();
                }
                __instance.selectedPanel = panel;
                SimulateSelect(__instance.selectedPanel);
                PluginDataManager.AnnouncementManager.ReadAnnnounement(a);
                NewsCountButton button = Object.FindObjectOfType<NewsCountButton>();
                if (button != null) AddModdedAnnuncementsToCount(button);
                SelectAnnouncement(__instance, a, ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
            }));
        });
        var newList = __instance.visibleAnnouncements.ToArray().ToList();
        newList.Sort((x, y) =>
        {
            DateTime xDate, yDate;
            bool xValid = false, yValid = false;

            if (x.announcement.Title != ModdedText && DateTime.TryParse(x.announcement.Date, out xDate)) xValid = true;
            else xValid = DateTime.TryParseExact(x.DateText.text, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out xDate);

            if (y.announcement.Title != ModdedText && DateTime.TryParse(y.announcement.Date, out yDate)) yValid = true;
            else yValid = DateTime.TryParseExact(y.DateText.text, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out yDate);

            if (!xValid && !yValid) return 0;

            if (!xValid) return 1;

            if (!yValid) return -1;

            return xDate.CompareTo(yDate);
        });
        __instance.visibleAnnouncements = new Il2CppSystem.Collections.Generic.List<AnnouncementPanel>();
        Vector3 localPosition = __instance.panelStartPos;
        newList.Reverse();
        newList.ForEach(a =>
        {
            __instance.visibleAnnouncements.Add(a);
            a.transform.localPosition = localPosition;
            localPosition.y -= 0.8f;
        });
        __instance.ListScroller.SetBoundsMax(0.8f * (float)(newList.Count + 1) - 2.512f, 0f);
        newList.Clear();
    }

    [QuickPrefix(typeof(NewsCountButton), nameof(NewsCountButton.UpdateCountText))]
    public static bool AddModdedAnnuncementsToCount(NewsCountButton __instance)
    {
        int num = DataManager.Player.Announcements.AllAnnouncements.Count - DataManager.Player.Announcements.AnnouncementsRead.Count;
        log.Log(LogLevel.All, $"au unread announcements: {num}");
        num += PluginDataManager.AnnouncementManager.GetUnReadAnnouncements().Count();
        log.Log(LogLevel.All, $"after we add our unread announcements: {num}");
        __instance.notifIcon.SetActive(num > 0);
        if (num > 9)
        {
            __instance.countText.text = "9+";
            return false;
        }
        __instance.countText.text = num.ToString();
        return false;
    }

    private static void SelectAnnouncement(AnnouncementPopUp __instance, Announcement announcementInfo, bool previewOnly)
    {
        // log.Debug("Updating announcement text for announcement ID {0}".Formatted(announcementInfo.Title));
        // if (!AnnouncementPopUp.IsSuccess(AnnouncementPopUp.UpdateState) && (announcement == null || announcement.Value.Number == 0))
        // {
        //     announcement = new Announcement?(DataManager.Player.Announcements.AllAnnouncements[0]);
        // }
        if (previewOnly)
        {
            log.Info("Announcement Setting preview");
            __instance.ListStateHUD.SetActive(true);
            string text;
            SelectableHyperLinkHelper.SanitizeLinkText(announcementInfo.BodyText, out text);
            __instance.AnnouncementBodyText.text = text;
        }
        else
        {
            log.Info("Announcement Setting full text");
            __instance.ListStateHUD.SetActive(false);
            ControllerManager.Instance.CloseOverlayMenu("Reading");
            ControllerManager.Instance.OpenOverlayMenu("Reading", __instance.ReadingBackButton);
            ControllerManager.Instance.OpenSpecificMenu("Reading");
            __instance.readingAnnouncement = true;
            SelectableHyperLinkHelper.DestroyGOs(__instance.selectableHyperLinks, "Reading");
            __instance.AnnouncementBodyText.text = SelectableHyperLinkHelper.DecomposeAnnouncementText(__instance.AnnouncementBodyText, __instance.selectableHyperLinks, "Reading", announcementInfo.BodyText);
        }
        __instance.Title.text = announcementInfo.Title;
        __instance.SubTitle.text = announcementInfo.Subtitle;
        __instance.DateString.text = announcementInfo.FormattedToLanguage();
        __instance.TextScroller.ScrollToTop();
        __instance.ManualScrollHelper.enabled = true;
        __instance.StartCoroutine(__instance.DelayedUpdateHyperlinkPositions());
    }

    private static void SimulateSelect(AnnouncementPanel panel)
    {
        panel.RightBorder.enabled = false;
        panel.Background.enabled = true;
        panel.Background.color = panel.SelectedColor;
        panel.NewIcon.enabled = false;
    }
}