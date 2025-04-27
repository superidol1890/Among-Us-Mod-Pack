using System;
using System.Collections.Generic;
using TownOfUs.Extensions;

namespace TownOfUs.CrewmateRoles.MedicMod
{
    public class DeadPlayer
    {
        public byte KillerId { get; set; }
        public byte PlayerId { get; set; }
        public DateTime KillTime { get; set; }
    }

    //body report class for when medic reports a body
    public class BodyReport
    {
        public PlayerControl Killer { get; set; }
        public PlayerControl Reporter { get; set; }
        public PlayerControl Body { get; set; }
        public float KillAge { get; set; }

        public static string ParseBodyReport(BodyReport br)
        {
            //System.Console.WriteLine(br.KillAge);
            if (br.KillAge > CustomGameOptions.MedicReportColorDuration * 1000)
                return
                    $"Body Report: The corpse is too old to gain information from. (Killed {Math.Round(br.KillAge / 1000)}s ago)";

            if (br.Killer.PlayerId == br.Body.PlayerId)
                return
                    $"Body Report: The kill appears to have been a suicide! (Killed {Math.Round(br.KillAge / 1000)}s ago)";

            var colors = new Dictionary<int, string>
            {
                {10, "darker"}, // Red
                {20, "darker"}, // Blue
                {17, "darker"}, // Green
                {6, "lighter"}, // Pink
                {12, "lighter"}, // Orange
                {31, "lighter"}, // Yellow
                {25, "darker"}, // Black
                {3, "lighter"}, // White
                {1, "darker"}, // Purple
                {19, "darker"}, // Brown
                {23, "lighter"}, // Cyan
                {33, "lighter"}, // Lime
                {9, "darker"}, // Maroon
                {30, "lighter"}, // Rose
                {29, "lighter"}, // Banana
                {4, "darker"}, // Grey
                {27, "darker"}, // Tan
                {7, "lighter"}, // Coral
                {8, "darker"}, // Melon
                {26, "darker"}, // Cocoa
                {21, "lighter"}, // Sky Blue
                {28, "lighter"}, // Biege
                {5, "darker"}, // Magenta
                {24, "lighter"}, // Aqua
                {2, "lighter"}, // Lilac
                {18, "darker"}, // Olive
                {22, "lighter"}, // Azure
                {0, "darker"}, // Plum
                {16, "darker"}, // Jungle
                {32, "lighter"}, // Mint
                {14, "lighter"}, // Lemon
                {15, "darker"}, // Macau
                {11, "darker"}, // Tawny
                {13, "lighter"}, // Gold
                {34, "lighter"}, // Rainbow
            };
            var typeOfColor = colors[br.Killer.GetDefaultOutfit().ColorId];
            return
                $"Body Report: The killer appears to be a {typeOfColor} color. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
        }
    }
}
