using LaunchpadReloaded.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Crewmate;

public class GamblerOptions : AbstractOptionGroup<GamblerRole>
{
    public override string GroupName => "Gambler";

    [ModdedNumberOption("Gamble Cooldown", 0, 60, 5, MiraNumberSuffixes.Seconds)]
    public float GambleCooldown { get; set; } = 25;

    [ModdedNumberOption("Gamble Uses", 0, 10, zeroInfinity: true)]
    public float GambleUses { get; set; } = 0;
}