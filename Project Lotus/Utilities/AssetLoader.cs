using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using VentLib.Utilities.Extensions;

namespace Lotus.Utilities;

public class AssetLoader
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(AssetLoader));

    private const string AssetPath = "Lotus.assets";
    private readonly Dictionary<string, LazySprite> cachedLazySprites = new();

    public static Sprite LoadSprite(string path, float pixelsPerUnit = 100f, bool linear = false, int mipMapLevel = 0, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        Sprite sprite;
        MemoryStream memoryStream = new();
        try
        {
            Stream? stream = assembly.GetManifestResourceStream(path);
            if (stream == null) throw new NullReferenceException($"Resource stream was null. Path: {path}");
            Texture2D texture = new(1, 1, TextureFormat.ARGB32, true, linear);
            stream.CopyTo(memoryStream);
            ImageConversion.LoadImage(texture, memoryStream.ToArray());
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.texture.requestedMipmapLevel = mipMapLevel;
        }
        catch (Exception)
        {
            log.Exception($"Error Loading Asset: \"{path}\"", "LoadImage");
            throw;
        }
        finally
        {
            memoryStream.Close();
        }

        return sprite;
    }
    public static TMP_FontAsset LoadFont(string path, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        try
        {
            using Stream? stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
            {
                log.Fatal($"Embedded font resource '{path}' not found.");
                return null!;
            }

            byte[] fontData = new byte[stream.Length];
            stream.Read(fontData, 0, fontData.Length);

            string tempFontPath = Path.Combine(Path.GetTempPath(), "tempEmbeddedFont.ttf");
            File.WriteAllBytes(tempFontPath, fontData);

            Font customFont = new(tempFontPath);
            TMP_FontAsset tmpFontAsset = TMP_FontAsset.CreateFontAsset(customFont);

            File.Delete(tempFontPath);
            stream.Dispose();

            return tmpFontAsset;
        }
        catch (Exception ex)
        {
            log.Exception($"Failed to load embedded font: {ex.Message}");
            return null!;
        }
    }

    public static bool ResourceExists(string path, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        foreach (var res in assembly.GetManifestResourceNames())
        {
            if (res == path) return true;
        }
        return false;
    }

    internal static Sprite LoadLotusSprite(string path, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0)
    {
        if (path.StartsWith('.')) path = AssetPath + path;
        else path = AssetPath + "." + path;
        return LoadSprite(path, pixelsPerUnit, linear, mipMapLevels);
    }

    public LazySprite LoadLazy(string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        return cachedLazySprites.GetOrCompute(resourcePath, () => new LazySprite(() => LoadSprite(resourcePath, pixelsPerUnit, linear, mipMapLevels, assembly)));
    }

    internal LazySprite LotusLoadLazy(string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        if (resourcePath.StartsWith('.')) resourcePath = AssetPath + resourcePath;
        else resourcePath = AssetPath + "." + resourcePath;
        return LoadLazy(resourcePath, pixelsPerUnit, linear, mipMapLevels, assembly);
    }
}