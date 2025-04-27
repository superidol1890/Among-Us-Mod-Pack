using Reactor.Localization.Utilities;
using UnityEngine;

namespace TownOfUs.RainbowMod
{
    public static class PalettePatch
    {
        public static void Load()
        {
            Palette.ColorNames = new[]
            {
                CustomStringName.CreateAndRegister("Plum"),
                StringNames.ColorPurple,
                CustomStringName.CreateAndRegister("Lilac"),
                StringNames.ColorWhite,
                StringNames.ColorGray,
                CustomStringName.CreateAndRegister("Magenta"),
                StringNames.ColorPink,
                StringNames.ColorCoral,
                CustomStringName.CreateAndRegister("Melon"),
                StringNames.ColorMaroon,
                StringNames.ColorRed,
                CustomStringName.CreateAndRegister("Tawny"),
                StringNames.ColorOrange,
                CustomStringName.CreateAndRegister("Gold"),
                CustomStringName.CreateAndRegister("Lemon"),
                CustomStringName.CreateAndRegister("Macau"),
                CustomStringName.CreateAndRegister("Jungle"),
                StringNames.ColorGreen,
                CustomStringName.CreateAndRegister("Olive"),
                StringNames.ColorBrown,
                StringNames.ColorBlue,
                CustomStringName.CreateAndRegister("Sky Blue"),
                CustomStringName.CreateAndRegister("Azure"),
                StringNames.ColorCyan,
                CustomStringName.CreateAndRegister("Aqua"),
                StringNames.ColorBlack,
                CustomStringName.CreateAndRegister("Cocoa"),
                StringNames.ColorTan,
                CustomStringName.CreateAndRegister("Beige"),
                StringNames.ColorBanana,
                StringNames.ColorRose,
                StringNames.ColorYellow,
                CustomStringName.CreateAndRegister("Mint"),
                StringNames.ColorLime,
                RainbowUtils.RainbowString
            };
            Palette.PlayerColors = new[]
            {
                new Color32(79, 0, 127, byte.MaxValue),//plum
                new Color32(107, 47, 188, byte.MaxValue),//purple
                new Color32(186, 161, 255, byte.MaxValue),//lilac
                new Color32(215, 225, 241, byte.MaxValue),//white
                Palette.FromHex(7701907),//gray
                new Color32(255, 0, 127, byte.MaxValue),//magenta
                new Color32(238, 84, 187, byte.MaxValue),//pink
                Palette.FromHex(14115940),//coral
                new Color32(168, 50, 62, byte.MaxValue),//melon
                Palette.FromHex(6233390),//maroon
                new Color32(198, 17, 17, byte.MaxValue),//red
                new Color32(205, 63, 0, byte.MaxValue),//tawny
                new Color32(240, 125, 13, byte.MaxValue),//orange
                new Color32(255, 207, 0, byte.MaxValue),//gold
                new Color32(207, 255, 0, byte.MaxValue),//lemon
                new Color32(0, 97, 93, byte.MaxValue),//macau
                new Color32(0, 47, 0, byte.MaxValue),//jungle
                new Color32(17, 128, 45, byte.MaxValue),//green
                new Color32(97, 114, 24, byte.MaxValue),//olive
                new Color32(113, 73, 30, byte.MaxValue),//brown
                new Color32(19, 46, 210, byte.MaxValue),//blue
                new Color32(61, 129, 255, byte.MaxValue),//sky blue
                new Color32(1, 166, 255, byte.MaxValue),//azure
                new Color32(56, byte.MaxValue, 221, byte.MaxValue),//cyan
                new Color32(61, 255, 181, byte.MaxValue),//aqua
                new Color32(63, 71, 78, byte.MaxValue),//black
                new Color32(60, 48, 44, byte.MaxValue),//cocoa
                Palette.FromHex(9537655),//tan
                new Color32(240, 211, 165, byte.MaxValue),//beige
                Palette.FromHex(15787944),//banana
                Palette.FromHex(15515859),//rose
                new Color32(246, 246, 87, byte.MaxValue),//yellow
                new Color32(151, 255, 151, byte.MaxValue),//mint
                new Color32(80, 240, 57, byte.MaxValue),//lime
                new Color32(0, 0, 0, byte.MaxValue),//rainbow
            };
            Palette.ShadowColors = new[]
            {
                new Color32(55, 0, 95, byte.MaxValue),//plum
                new Color32(59, 23, 124, byte.MaxValue),//purple
                new Color32(93, 81, 128, byte.MaxValue),//lilac
                new Color32(132, 149, 192, byte.MaxValue),//white
                Palette.FromHex(4609636),//gray
                new Color32(191, 0, 95, byte.MaxValue),//magenta
                new Color32(172, 43, 174, byte.MaxValue),//pink
                Palette.FromHex(11813730),//coral
                new Color32(101, 30, 37, byte.MaxValue),//melon
                Palette.FromHex(4263706),//maroon
                new Color32(122, 8, 56, byte.MaxValue),//red
                new Color32(141, 31, 0, byte.MaxValue),//tawny
                new Color32(180, 62, 21, byte.MaxValue),//orange
                new Color32(191, 143, 0, byte.MaxValue),//gold
                new Color32(143, 191, 61, byte.MaxValue),//lemon
                new Color32(0, 65, 61, byte.MaxValue),//macau
                new Color32(0, 23, 0, byte.MaxValue),//jungle
                new Color32(10, 77, 46, byte.MaxValue),//green
                new Color32(66, 91, 15, byte.MaxValue),//olive
                new Color32(94, 38, 21, byte.MaxValue),//brown
                new Color32(9, 21, 142, byte.MaxValue),//blue
                new Color32(31, 65, 128, byte.MaxValue),//sky blue
                new Color32(17, 104, 151, byte.MaxValue),//azure
                new Color32(36, 169, 191, byte.MaxValue),//cyan
                new Color32(31, 128, 91, byte.MaxValue),//aqua
                new Color32(30, 31, 38, byte.MaxValue),//black
                new Color32(30, 24, 22, byte.MaxValue),//cocoa
                Palette.FromHex(5325118),//tan
                new Color32(120, 106, 83, byte.MaxValue),//beige
                Palette.FromHex(13810825),//banana
                Palette.FromHex(14586547),//rose
                new Color32(195, 136, 34, byte.MaxValue),//yellow
                new Color32(109, 191, 109, byte.MaxValue),//mint
                new Color32(21, 168, 66, byte.MaxValue),//lime
                new Color32(0, 0, 0, byte.MaxValue),//rainbow
            };
        }
    }
}
