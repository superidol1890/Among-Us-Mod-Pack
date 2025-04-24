using LaunchpadReloaded.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadReloaded.Roles.Impostor;

public class SurgeonRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    public string RoleName => "Surgeon";
    public string RoleDescription => "Poison other players and dissect bodies";
    public string RoleLongDescription => "You can poison players resulting in their death\nand you can dissect bodies to make them unreportable.";
    public Color RoleColor => LaunchpadPalette.SurgeonColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.DissectButton,
        UseVanillaKillButton = false,
        OptionsScreenshot = LaunchpadAssets.SurgeonBanner,
    };
}