using LaunchpadReloaded.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Impostor;

public class HitmanOptions : AbstractOptionGroup<HitmanRole>
{
    public override string GroupName => "Hitman";

    [ModdedNumberOption("Deadlock Cooldown", 20, 120, 5, MiraNumberSuffixes.Seconds)]
    public float DeadlockCooldown { get; set; } = 40;

    [ModdedNumberOption("Deadlock Uses", 0, 12, 2, zeroInfinity: true)]
    public float DeadlockUses { get; set; } = 3;

    [ModdedNumberOption("Deadlock Mark Limit", 1, 12, 1, zeroInfinity: false)]
    public float MarkLimit { get; set; } = 2;

    [ModdedNumberOption("Deadlock Duration", 15, 50, 2.5f)]
    public float DeadlockDuration { get; set; } = 20;
}