using System.Collections.Generic;
using UnityEngine;
using System;

namespace TownOfUs.Roles
{
    public class Medic : Role
    {
        public readonly List<GameObject> Buttons = new List<GameObject>();
        public Dictionary<int, string> LightDarkColors = new Dictionary<int, string>();
        public DateTime StartingCooldown { get; set; }
        public Medic(PlayerControl player) : base(player)
        {
            Name = "Medic";
            ImpostorText = () => "Create A Shield To Protect A Crewmate";
            TaskText = () => "Protect a Crewmate with a shield";
            Color = Patches.Colors.Medic;
            StartingCooldown = DateTime.UtcNow;
            RoleType = RoleEnum.Medic;
            AddToRoleHistory(RoleType);
            ShieldedPlayer = null;

            LightDarkColors.Add(10, "darker"); // Red
            LightDarkColors.Add(20, "darker"); // Blue
            LightDarkColors.Add(17, "darker"); // Green
            LightDarkColors.Add(6, "lighter"); // Pink
            LightDarkColors.Add(12, "lighter"); // Orange
            LightDarkColors.Add(31, "lighter"); // Yellow
            LightDarkColors.Add(25, "darker"); // Black
            LightDarkColors.Add(3, "lighter"); // White
            LightDarkColors.Add(1, "darker"); // Purple
            LightDarkColors.Add(19, "darker"); // Brown
            LightDarkColors.Add(23, "lighter"); // Cyan
            LightDarkColors.Add(33, "lighter"); // Lime
            LightDarkColors.Add(9, "darker"); // Maroon
            LightDarkColors.Add(30, "lighter"); // Rose
            LightDarkColors.Add(29, "lighter"); // Banana
            LightDarkColors.Add(4, "darker"); // Grey
            LightDarkColors.Add(27, "darker"); // Tan
            LightDarkColors.Add(7, "lighter"); // Coral
            LightDarkColors.Add(8, "darker"); // Melon
            LightDarkColors.Add(26, "darker"); // Cocoa
            LightDarkColors.Add(21, "lighter"); // Sky Blue
            LightDarkColors.Add(28, "lighter"); // Biege
            LightDarkColors.Add(5, "darker"); // Magenta
            LightDarkColors.Add(24, "lighter"); // Aqua
            LightDarkColors.Add(2, "lighter"); // Lilac
            LightDarkColors.Add(18, "darker"); // Olive
            LightDarkColors.Add(22, "lighter"); // Azure
            LightDarkColors.Add(0, "darker"); // Plum
            LightDarkColors.Add(16, "darker"); // Jungle
            LightDarkColors.Add(32, "lighter"); // Mint
            LightDarkColors.Add(14, "lighter"); // Lemon
            LightDarkColors.Add(15, "darker"); // Macau
            LightDarkColors.Add(11, "darker"); // Tawny
            LightDarkColors.Add(13, "lighter"); // Gold
            LightDarkColors.Add(34, "lighter"); // Rainbow
        }
        public float StartTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - StartingCooldown;
            var num = 10000f;
            var flag2 = num - (float)timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float)timeSpan.TotalMilliseconds) / 1000f;
        }

        public PlayerControl ClosestPlayer;
        public PlayerControl ShieldedPlayer { get; set; }
    }
}