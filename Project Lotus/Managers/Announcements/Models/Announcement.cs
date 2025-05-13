using System;
using AmongUs.Data;

namespace Lotus.Managers.Announcements.Models;
public class Announcement
{
    public string Title { get; set; } = null!;
    public string ShortTitle { get; set; } = null!;
    public string Subtitle { get; set; } = null!;
    public string BodyText { get; set; } = null!;
    public bool DevOnly { get; set; } = false;
    public DateOnly? Date { get; set; }

    public string FormattedDate() => (Date ?? DateOnly.MinValue).ToDateTime(new TimeOnly(12, 0)).ToString("MM/dd/yyyy");
    public string FormattedToLanguage() => (Date ?? DateOnly.MinValue)
        .ToDateTime(new TimeOnly(12, 0))
        .ToLocalTime()
        .ToString(DestroyableSingleton<TranslationController>.Instance.dateFormats[DataManager.Settings.Language.CurrentLanguage]);
}