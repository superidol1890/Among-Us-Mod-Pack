using System.IO;
using System.Reflection;
using Lotus.Extensions;
using Lotus.GUI;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lotus.Roles.Outfit;

public class OutfitFile
{
    private static IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    public int Color { get; set; } = 0;
    public string Skin { get; set; } = "";
    public string Visor { get; set; } = "";
    public string Hat { get; set; } = "";
    public string Pet { get; set; } = "";
    public bool ShowDead { get; set; } = false;

    public NetworkedPlayerInfo.PlayerOutfit ToPlayerOutfit()
    {
        return new NetworkedPlayerInfo.PlayerOutfit()
        {
            ColorId = Color,
            SkinId = Skin,
            VisorId = Visor,
            HatId = Hat,
            PetId = Pet
        };
    }

    public static OutfitFile FromManifestFile(string manifestResource, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(manifestResource);
        if (stream == null) return null!;
        string result;
        using (StreamReader reader = new(stream)) result = reader.ReadToEnd();
        stream.Dispose();
        return FromPlainText(result);
    }

    public static OutfitFile FromAssetBundle(string assetName, AssetBundle? bundle = null)
    {
        bundle ??= LotusAssets.Bundle;
        TextAsset? textAsset = bundle.LoadAsset<TextAsset>(assetName);
        if (textAsset == null) return null!;
        return FromPlainText(textAsset.text);
    }

    public static OutfitFile FromFileInfo(FileInfo file)
    {
        string result;
        using (StreamReader reader = new(file.Open(FileMode.Open))) result = reader.ReadToEnd();
        return FromPlainText(result);
    }

    public static OutfitFile FromPlainText(string yamlText) => deserializer.Deserialize<OutfitFile>(yamlText);
}