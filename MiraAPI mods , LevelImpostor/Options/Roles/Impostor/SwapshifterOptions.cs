using LaunchpadReloaded.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Impostor;

public class SwapshifterOptions : AbstractOptionGroup<SwapshifterRole>
{
    public override string GroupName => "Swapshifter";

    [ModdedNumberOption("Swap Cooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float SwapCooldown { get; set; } = 30;

    [ModdedNumberOption("Swap Duration", 0, 70, 5, MiraNumberSuffixes.Seconds)]
    public float SwapDuration { get; set; } = 15;

    [ModdedNumberOption("Swap Uses", 0, 8, zeroInfinity: true)]
    public float SwapUses { get; set; } = 3;

    [ModdedToggleOption("Can Swap with Impostors")]
    public bool CanSwapImpostors { get; set; } = true;
}