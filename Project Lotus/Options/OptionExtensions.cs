using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VentLib.Options.UI;

namespace Lotus.Options;

public static class OptionExtensions
{
    public static GameOptionBuilder Builder(this GameOptionBuilder builder, string key, Color? color = null) => builder.Key(key).Color(color ?? Color.white);
}