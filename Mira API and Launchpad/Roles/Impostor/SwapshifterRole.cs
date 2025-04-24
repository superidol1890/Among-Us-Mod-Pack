using LaunchpadReloaded.Buttons.Impostor;
using LaunchpadReloaded.Features;
using MiraAPI.Hud;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadReloaded.Roles.Impostor;

public class SwapshifterRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    public string RoleName => "Swapshifter";
    public string RoleDescription => "Shift and swap into other players.";
    public string RoleLongDescription => RoleDescription + "\nThis can help you frame players and disguise kills.";
    public Color RoleColor => LaunchpadPalette.SwapperColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.SwapButton,
        OptionsScreenshot = LaunchpadAssets.HackerBanner,
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

        if (CustomButtonSingleton<SwapButton>.Instance.EffectActive)
        {
            CustomButtonSingleton<SwapButton>.Instance.OnEffectEnd();
        }
    }
}
