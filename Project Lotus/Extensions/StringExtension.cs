using System.Text.RegularExpressions;
using Lotus.Utilities;
using UnityEngine;

namespace Lotus.Extensions;

public static class StringExtension
{
    public static string RemoveColorTags(this string str) => Regex.Replace(str, "<[^size>]*?>", "");

    public static Color? ToColor(this string str) => Utils.ConvertHexToColor(str);

    public static ulong SemiConsistentHash(this object obj)
    {
        string read = obj.ToString() ?? "null";
        ulong hashedValue = 3074457345618258791ul;
        foreach (var ch in read)
        {
            hashedValue += ch;
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    public static string ReplaceN(this string str, string oldText, string newText, int count)
    {
        Regex regex = new Regex(Regex.Escape(oldText));
        return regex.Replace(str, newText, count);
    }
}