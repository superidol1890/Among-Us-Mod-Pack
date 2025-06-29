using LaunchpadReloaded.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Crewmate;

public class SheriffOptions : AbstractOptionGroup<SheriffRole>
{
    public override string GroupName => "Sheriff";

    [ModdedNumberOption("Shot Cooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float ShotCooldown { get; set; } = 45;

    [ModdedNumberOption("Shots Per Game", 1, 10)]
    public float ShotsPerGame { get; set; } = 3;

    [ModdedToggleOption("Should Crewmate Die")]
    public bool ShouldCrewmateDie { get; set; } = false;
}