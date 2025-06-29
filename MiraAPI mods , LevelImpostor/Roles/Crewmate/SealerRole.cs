using Il2CppInterop.Runtime.Attributes;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Features;
using MiraAPI.Roles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaunchpadReloaded.Roles.Crewmate;

public class SealerRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public string RoleName => "Sealer";
    public string RoleDescription => "Seal vents around the map.";
    public string RoleLongDescription => "Seal vents around the map.\nThis will prevent anyone from entering the vent.";
    public Color RoleColor => LaunchpadPalette.SealerColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.SealButton,
        OptionsScreenshot = LaunchpadAssets.MedicBanner,
    };

    [HideFromIl2Cpp]
    public List<SealedVentComponent> SealedVents { get; } = [];
}
