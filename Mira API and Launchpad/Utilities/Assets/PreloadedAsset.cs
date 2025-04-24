using System;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// An implementation for creating loadable assets from existing assets.
/// </summary>
/// <inheritdoc cref="LoadableAsset{T}"/>
/// <inheritdoc />
public class PreloadedAsset<T>(T asset) : LoadableAsset<T> where T : UnityEngine.Object
{
    private readonly T _loadedAsset = asset ?? throw new ArgumentNullException(nameof(asset));

    /// <inheritdoc />
    public override T LoadAsset()
    {
        return _loadedAsset;
    }
}
