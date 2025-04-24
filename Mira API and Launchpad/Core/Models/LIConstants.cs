﻿namespace LevelImposter.Core;

/// <summary>
///     A list of constants used within LevelImposter
/// </summary>
public static class LIConstants
{
    public const StringNames MAP_STRING_NAME = (StringNames)392001; // StringName that placeholdes the map names
    public const string MAP_NAME = "Random Custom Map"; // Name to populate Constants.MapNames
    public const float PLAYER_POS = -5.0f; // Z value of the player
    public const int MAX_LOAD_TIME = 10; // Maximum time to build map before aborting
    public const int MAX_CONNECTION_TIMEOUT = 10; // Maximum time to wait for a client connection

    public const int
        ELEM_WARN_TIME = 200; // Time to warn the user (in ms) when an element is taking too long to load

    public const bool FREEPLAY_FLUSH_CACHE = true; // Whether to flush the cache when entering freeplay maps
    public const bool OVERRIDE_HORSE_MODE = false; // Whether to override the horse mode setting
    public const bool ENABLE_HORSE_MODE = false; // Whether to enable horse mode (requires OVERRIDE_HORSE_MODE=true)
}