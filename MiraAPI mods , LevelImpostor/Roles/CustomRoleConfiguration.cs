using AmongUs.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Used to configure the specific settings of a role.
/// </summary>
public record struct CustomRoleConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRoleConfiguration"/> struct.
    /// </summary>
    /// <param name="role">The role in which you are configuring.</param>
    public CustomRoleConfiguration(ICustomRole role)
    {
        var roleBehaviour = role as RoleBehaviour;

        AffectedByLightOnAirship = role.Team == ModdedRoleTeams.Crewmate;
        KillButtonOutlineColor = role.Team switch
        {
            ModdedRoleTeams.Impostor => Palette.ImpostorRed,
            ModdedRoleTeams.Crewmate => Palette.CrewmateBlue,
            _ => role.RoleColor,
        };
        GhostRole = role.Team is ModdedRoleTeams.Impostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;
        CanGetKilled = roleBehaviour?.IsDead == false && role.Team is not ModdedRoleTeams.Impostor;
        UseVanillaKillButton = role.Team is ModdedRoleTeams.Impostor;
        CanUseVent = role.Team is ModdedRoleTeams.Impostor;
        CanUseSabotage = role.Team is ModdedRoleTeams.Impostor;
        TasksCountForProgress = role.Team is ModdedRoleTeams.Crewmate;
        HideSettings = roleBehaviour?.IsDead == true;
        ShowInFreeplay = roleBehaviour?.IsDead == false;
        IntroSound = role.Team is ModdedRoleTeams.Crewmate
            ? CustomRoleManager.CrewmateIntroSound
            : CustomRoleManager.ImpostorIntroSound;
    }

    /// <summary>
    /// Gets or sets the hard limit of players that can have this role. This property is used to set a limit in the Role Options menu. If set to 0, the role will not be assigned at start.
    /// </summary>
    public int MaxRoleCount { get; set; } = 15;

    /// <summary>
    /// Gets or sets the default role count.
    /// </summary>
    public int DefaultRoleCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the default role chance.
    /// </summary>
    public int DefaultChance { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the chance option can be changed or not.
    /// </summary>
    public bool CanModifyChance { get; set; } = true;

    /// <summary>
    /// Gets or sets the Sprite used for the Role Options menu screenshot.
    /// </summary>
    public LoadableAsset<Sprite>? OptionsScreenshot { get; set; } = null;

    /// <summary>
    /// Gets or sets the Sprite used for the Role Icon.
    /// </summary>
    public LoadableAsset<Sprite>? Icon { get; set; } = null;

    /// <summary>
    /// Gets or sets the Intro sound for the Role.
    /// </summary>
    public LoadableAsset<AudioClip>? IntroSound { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether the role is affected by light affectors on Airship.
    /// </summary>
    public bool AffectedByLightOnAirship { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role can be killed by vanilla murder system.
    /// </summary>
    public bool CanGetKilled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role should use the vanilla kill button.
    /// </summary>
    public bool UseVanillaKillButton { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role can use vents.
    /// </summary>
    public bool CanUseVent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role can use the sabotage button.
    /// </summary>
    public bool CanUseSabotage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role's tasks count towards task progress.
    /// </summary>
    public bool TasksCountForProgress { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role should show up in the Role Options menu.
    /// </summary>
    public bool HideSettings { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role should show up in the Freeplay Role Selection menu.
    /// </summary>
    public bool ShowInFreeplay { get; set; }

    /// <summary>
    /// Gets or sets the outline color for the KillButton if <see cref="UseVanillaKillButton"/> is true.
    /// </summary>
    public Color KillButtonOutlineColor { get; set; }

    /// <summary>
    /// Gets or sets the role hint style. See <see cref="RoleHintType"/> enum for all options.
    /// </summary>
    public RoleHintType RoleHintType { get; set; } = RoleHintType.RoleTab;

    /// <summary>
    /// Gets or sets the Ghost role that is applied when the player is killed.
    /// </summary>
    public RoleTypes GhostRole { get; set; }
}
