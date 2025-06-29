using LaunchpadReloaded.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadReloaded.Roles.Crewmate;

public class GamblerRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public string RoleName => "Gambler";
    public string RoleDescription => "Guess a player's role to reveal it!";
    public string RoleLongDescription => "Guess a player's role to reveal it!\nHowever, if you get it incorrect, you will die.";
    public Color RoleColor => LaunchpadPalette.GamblerColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.GambleButton,
        OptionsScreenshot = LaunchpadAssets.DetectiveBanner,
    };
}
