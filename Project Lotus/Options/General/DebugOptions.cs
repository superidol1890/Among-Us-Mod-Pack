using System.Collections.Generic;
using System.Linq;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities.Extensions;

namespace Lotus.Options.General;

[Localized(ModConstants.Options)]
public class DebugOptions
{
    private static Color _optionColor = new(1f, 0.59f, 0.38f);
    private static List<GameOption> additionalOptions = new();

    public bool NoGameEnd;
    public bool NameBasedRoleAssignment;

    public List<GameOption> AllOptions = new();

    public DebugOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(DebugOptionTranslations.DebugOptionTitle)
            .Color(_optionColor)
            .Build());

        AllOptions.Add(Builder("No Game End")
            .Name(DebugOptionTranslations.NoGameEndText)
            .BindBool(b => NoGameEnd = b)
            .IsHeader(true)
            .Build());

        AllOptions.Add(Builder("Name Based Role Assignment")
            .Name(DebugOptionTranslations.NameBasedRoleAssignmentText)
            .BindBool(b => NameBasedRoleAssignment = b)
            .Build());

        // AllOptions.Add(Builder("Advanced Role Assignment")
        //     .Name(DebugOptionTranslations.AdvancedRoleAssignment)
        //     .BindBool(b => ProjectLotus.AdvancedRoleAssignment = b)
        //     .Build());

        AllOptions.AddRange(additionalOptions);
        AllOptions.Where(o => !o.Attributes.ContainsKey("Title")).ForEach(o => GeneralOptions.StandardOptionManager.Register(o, VentLib.Options.OptionLoadMode.LoadOrCreate));
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

    [Localized("Debug")]
    private static class DebugOptionTranslations
    {
        [Localized("SectionTitle")]
        public static string DebugOptionTitle = "Debug Options";

        [Localized("NoGameEnd")]
        public static string NoGameEndText = "Prevent Game Over";

        [Localized("NameRoleAssignment")]
        public static string NameBasedRoleAssignmentText = "Name-Based Role Assignment";

        [Localized("AdvancedRoleAssignment")]
        public static string AdvancedRoleAssignment = "Advanced Role Assignment";
    }
}