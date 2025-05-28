using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Lotus.GUI;
using Lotus.Managers.Announcements.Helpers;
using Lotus.Managers.Announcements.Models;
using UnityEngine;
using VentLib.Utilities.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lotus.Managers.Announcements;

public class CustomAnnouncementManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(CustomAnnouncementManager));
    private Dictionary<string, Announcement> CustomAnnouncements;
    private ReadAnnouncements readAnnouncements;
    private FileInfo announceFile;
    private static readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .WithTypeConverter(new DateOnlyYamlConverter())  // Register custom DateOnly converter
        .Build();

    private static readonly ISerializer serializer = new SerializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    public CustomAnnouncementManager(FileInfo readAnnouncementsFileInfo)
    {
        try
        {
            string[] assetNames = LotusAssets.Bundle.GetAllAssetNames()
                .Where(name => name.Contains("announcements/") && name.EndsWith(".yaml"))
                .ToArray();

            CustomAnnouncements = assetNames
                .Select(s =>
                {
                    string fileName = s.Replace("announcements/", "").Replace(".yaml", "");
                    return (fileName, LoadFromTextAsset(LotusAssets.LoadAsset<TextAsset>(s)));
                })
                .Where(a => a.Item2 != null)
                .ToDict(a => a.fileName, a => a.Item2)!;
        }
        catch (Exception exception)
        {
            log.Exception("Error loading in manifest (global) announcements.", exception);
            CustomAnnouncements = new Dictionary<string, Announcement>();
        }
        announceFile = readAnnouncementsFileInfo;
        if (!announceFile.Exists) WriteFile(new ReadAnnouncements());

        string content;
        using (StreamReader reader = new(announceFile.Open(FileMode.OpenOrCreate))) content = reader.ReadToEnd();
        readAnnouncements = deserializer.Deserialize<ReadAnnouncements>(content);
    }

    public bool HasNewAnnouncements
    {
        get => CustomAnnouncements.Keys.Any(id => !readAnnouncements.Read.Contains(id));
    }

    public IEnumerable<Announcement> GetUnReadAnnouncements() => CustomAnnouncements.Where(kvp => !HasReadAnnouncement(kvp.Value)).Select(kvp => kvp.Value);

    public void AddAnnouncements(IEnumerable<Announcement> announcements)
    {
        announcements.Where(a => !string.IsNullOrEmpty(a.Title)).ForEach(a => CustomAnnouncements.Add(a.Title, a));
    }

    public void AddAnnouncements(params Announcement[] announcements) => AddAnnouncements((IEnumerable<Announcement>)announcements);

    public void AddAnnouncements(DirectoryInfo announcementDirectory)
    {
        AddAnnouncements(
            announcementDirectory.GetFiles("*.yaml")
            .Select(f =>
            {
                try
                {
                    string fileName = f.Name.Replace(".yaml", "");
                    return LoadFromFileInfo(f);
                }
                catch (Exception exception)
                {
                    log.Exception($"Error loading title file: {f.Name}.", exception);
                    return new Announcement();
                }
            }));
    }

    public bool HasReadAnnouncement(Announcement announcement)
    {
        string key = CustomAnnouncements.FirstOrDefault(kvp => kvp.Value == announcement).Key;
        if (string.IsNullOrEmpty(key)) return false;
        return readAnnouncements.Read.Contains(key);
    }

    internal void ReadAnnnounement(Announcement announcement)
    {
        string key = CustomAnnouncements.FirstOrDefault(kvp => kvp.Value == announcement).Key;
        if (readAnnouncements.Read.Contains(key)) return;
        readAnnouncements.Read.Add(key);
        WriteFile(readAnnouncements);
    }

    internal IEnumerable<Announcement> GetAnnouncements()
    {
        // remove announcements that dont exist.
        // at this point addons should have already added their announcements to the list
        readAnnouncements.Read.RemoveAll(id => !CustomAnnouncements.ContainsKey(id));
        WriteFile(readAnnouncements);
        return CustomAnnouncements.Values;
    }

    public static Announcement? LoadAnnouncementFromManifest(string manifestResource)
    {
        using Stream? stream = Assembly.GetCallingAssembly().GetManifestResourceStream(manifestResource);
        return stream == null ? null : LoadFromStream(stream);
    }

    public static Announcement LoadFromFileInfo(FileInfo file) => LoadFromStream(file.Open(FileMode.Open));

    public static Announcement? LoadFromTextAsset(TextAsset textAsset)
    {
        if (textAsset == null) return null!;
        string result = textAsset.text;
        return deserializer.Deserialize<Announcement>(result);
    }

    private static Announcement LoadFromStream(Stream stream)
    {
        string result;
        using (StreamReader reader = new(stream)) result = reader.ReadToEnd();
        stream.Dispose();

        return deserializer.Deserialize<Announcement>(result);
    }
    private void WriteFile(ReadAnnouncements newFile)
    {
        string emptyFile = serializer.Serialize(newFile);
        using FileStream stream = announceFile.Open(FileMode.Create);
        stream.Write(Encoding.UTF8.GetBytes(emptyFile));
        stream.Dispose();
    }
}