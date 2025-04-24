using LaunchpadReloaded.Buttons.Crewmate;
using LaunchpadReloaded.Features;
using MiraAPI.Hud;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadReloaded.Roles.Crewmate;

public class CaptainRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public string RoleName => "Captain";
    public string RoleDescription => "Protect the crew with your abilities";
    public string RoleLongDescription => "Use your zoom ability to keep an eye on the crew\n And call meetings from any location!";
    public Color RoleColor => LaunchpadPalette.CaptainColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.ZoomButton,
        OptionsScreenshot = LaunchpadAssets.CaptainBanner,
    };

    public override void OnDeath(DeathReason reason)
    {
        Deinitialize(Player);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        if (!targetPlayer.AmOwner)
        {
            return;
        }

        if (CustomButtonSingleton<ZoomButton>.Instance.EffectActive)
        {
            CustomButtonSingleton<ZoomButton>.Instance.OnEffectEnd();
        }
    }
}