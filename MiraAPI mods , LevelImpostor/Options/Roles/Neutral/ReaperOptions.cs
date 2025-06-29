using LaunchpadReloaded.Roles.Outcast;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Neutral;

public class ReaperOptions : AbstractOptionGroup<ReaperRole>
{
    public override string GroupName => "Reaper";

    [ModdedNumberOption("Collections To Win", 2, 8)]
    public float SoulCollections { get; set; } = 3;

    [ModdedNumberOption("Collect Cooldown", 0, 60, 5, MiraNumberSuffixes.Seconds)]
    public float CollectCooldown { get; set; } = 20;
}