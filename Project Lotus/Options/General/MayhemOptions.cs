using System;
using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.GameModes.Standard;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Lotus.Options.General;

[Localized(ModConstants.Options)]
public class MayhemOptions
{
    private static Color _optionColor = new(0.84f, 0.8f, 1f);
    private static List<GameOption> additionalOptions = new();

    public bool AllRolesCanVent;
    public bool CamoComms;

    public bool UseRandomSpawn => randomSpawnOn && Game.CurrentGameMode is StandardGameMode;
    private bool randomSpawnOn;

    public List<GameOption> AllOptions = new();

    public MayhemOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(Translations.MayhemOptionTitle)
            .Color(_optionColor)
            .Build());

        AllOptions.Add(Builder("Random Spawn")
            .Name(Translations.RandomSpawnText)
            .BindBool(b => randomSpawnOn = b)
            .Build());

        // AllOptions.Add(Builder("Camo Comms")
        //     .Name(Translations.CamoCommText)
        //     .BindBool(b => CamoComms = b)
        //     .Build());

        AllOptions.AddRange(additionalOptions);
    }

    /// <summary>
    /// Adds additional options to be registered when this group of options is loaded. This is mostly used for ordering
    /// in the main menu, as options passed in here will be rendered along with this group.
    /// </summary>
    /// <param name="option">Option to render</param>
    public static void AddAdditionalOption(GameOption option)
    {
        additionalOptions.Add(option);
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().AddBoolean(false).Builder(key, _optionColor);

    [Localized("Mayhem")]
    public static class Translations
    {
        [Localized("SectionTitle")]
        public static string MayhemOptionTitle = "Mayhem Options";

        [Localized("RandomMaps")]
        public static string RandomMapModeText = "Enable Random Maps";

        [Localized("RandomSpawn")]
        public static string RandomSpawnText = "Random Spawn";

        [Localized("CamoComms")]
        public static string CamoCommText = "Camo Comms";

        [Localized("AllRolesCanVent")]
        public static string AllRolesVentText = "All Roles Can Vent";

        [Localized("Skeld")] public static string MapNameSkeld = "Skeld";
        [Localized("Mira")] public static string MapNameMira = "Mira";
        [Localized("Polus")] public static string MapNamePolus = "Polus";
        [Localized("Airship")] public static string MapNameAirship = "Airship";
    }
}