using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles.EnergyThiefOptions;

public class EnergyThiefOptions : AbstractOptionGroup<EnergyThief>
{
    public override string GroupName => "Energy Thief";

    [ModdedNumberOption("Drain Cooldown", min: 10, max: 20, suffixType: MiraNumberSuffixes.Seconds)]
    public float DrainCooldown { get; set; } = 15f;

    [ModdedNumberOption("Drain Max Uses", min: 3, max: 5)]
    public float DrainMaxUses { get; set; } = 3f;
}