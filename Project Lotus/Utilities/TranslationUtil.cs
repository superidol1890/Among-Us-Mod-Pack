using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.Utilities;

public class TranslationUtil
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(TranslationUtil));
    private static Regex taggedStringRegex = new("(\\S+::\\d+)");

    /// <summary>
    /// Adds color to the specified tags (::).
    /// </summary>
    /// <param name="input">.The string to add color to.</param>
    /// <returns></returns>
    public static string Colorize(string input, params Color[] colors)
    {
        try
        {
            string[] tagStrings = taggedStringRegex.Matches(input).Select(m => m.Value).ToArray();

            string[] replacements = tagStrings.Select(v => v.Split("::"))
                .Select(va => colors[int.Parse(va[1])].Colorize(va[0])).ToArray();

            for (int index = 0; index < tagStrings.Length; index++)
            {
                string tagString = tagStrings[index];
                input = input.Replace(tagString, replacements[index]);
            }

        }
        catch (Exception exception)
        {
            log.Exception("Error colorizing message!", exception);
        }

        return input;
    }

    /// <summary>
    /// Removes color (::) tags from a string
    /// </summary>
    /// <param name="input">.The string to remove from.</param>
    /// <returns></returns>
    public static string Remove(string input)
    {
        try
        {
            string[] tagStrings = taggedStringRegex.Matches(input).Select(m => m.Value).ToArray();

            string[] plainTexts = tagStrings.Select(v => v.Split("::")[0]).ToArray();

            for (int index = 0; index < tagStrings.Length; index++)
            {
                string tagString = tagStrings[index];
                input = input.Replace(tagString, plainTexts[index]);
            }
        }
        catch (Exception exception)
        {
            log.Exception("Error removing color tags from message!", exception);
        }

        return input;
    }
}