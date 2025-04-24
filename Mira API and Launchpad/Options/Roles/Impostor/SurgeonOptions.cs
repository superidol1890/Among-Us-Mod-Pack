using LaunchpadReloaded.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Impostor;

public class SurgeonOptions : AbstractOptionGroup<SurgeonRole>
{
    public override string GroupName => "Surgeon";

    [ModdedNumberOption("Inject Cooldown", 0, 60, 5, MiraNumberSuffixes.Seconds)]
    public float InjectCooldown { get; set; } = 10f;

    [ModdedNumberOption("Inject Uses", 0, 10, zeroInfinity: true)]
    public float InjectUses { get; set; } = 0;

    [ModdedNumberOption("Dissect Cooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float DissectCooldown { get; set; } = 35f;

    [ModdedNumberOption("Dissect Uses", 0, 10, zeroInfinity: true)]
    public float DissectUses { get; set; } = 0;

    [ModdedNumberOption("Poison Death Delay", 5, 60, 5, MiraNumberSuffixes.Seconds)]
    public float PoisonDelay { get; set; } = 10;

    //    [ModdedToggleOption("Can Use Standard Kill")] NEED FIX
    //    public bool StandardKill { get; set; } = false; NEED FIX
}