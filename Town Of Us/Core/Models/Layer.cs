﻿namespace LevelImposter.Core;

internal enum Layer
{
    /// <summary>
    ///     Only visible in light, interacts with ghosts
    /// </summary>
    Default,
    TransparentFX,
    IgnoreRaycast,

    /// <summary>
    ///     Made-up layer for physics objects so they collide with each other
    ///     Camera is modified to render this layer
    /// </summary>
    Physics,

    Water,

    /// <summary>
    ///     Full-brightness and always visible
    /// </summary>
    UI,
    Players = 8,
    Ship,

    /// <summary>
    ///     Only visible in shadow, blocks light
    /// </summary>
    Shadow,

    /// <summary>
    ///     Automatically hidden by <c>util-display</c> objects
    /// </summary>
    Objects,

    ShortObjects,
    IlluminatedBlocking,
    Ghost,
    UICollider,
    DrawShadows,
    KeyMapper,
    MusicTriggers,
    Notifications
}