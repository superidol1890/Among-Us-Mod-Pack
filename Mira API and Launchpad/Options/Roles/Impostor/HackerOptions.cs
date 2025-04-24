using LaunchpadReloaded.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Impostor;

public class HackerOptions : AbstractOptionGroup<HackerRole>
{
    public override string GroupName => "Hacker";

    [ModdedNumberOption("Hack Cooldown", 10, 300, 10, MiraNumberSuffixes.Seconds)]
    public float HackCooldown { get; set; } = 60;

    [ModdedNumberOption("Hack Duration", 10, 500, 10, MiraNumberSuffixes.Seconds)]
    public float HackDuration { get; set; } = 90;

    [ModdedNumberOption("Hacks Per Game", 1, 8)]
    public float HackUses { get; set; } = 2;

    [ModdedNumberOption("Map Cooldown", 0, 40, 3, MiraNumberSuffixes.Seconds)]
    public float MapCooldown { get; set; } = 10;

    [ModdedNumberOption("Map Duration", 1, 30, 3, MiraNumberSuffixes.Seconds)]
    public float MapDuration { get; set; } = 3;
}