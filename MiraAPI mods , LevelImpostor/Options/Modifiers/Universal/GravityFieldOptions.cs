using System;
using LaunchpadReloaded.Modifiers.Game;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Modifiers.Universal;

public class GravityFieldOptions : AbstractOptionGroup<GravityModifier>
{
    public override string GroupName => "Gravity Field";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<UniversalModifierOptions>.Instance.GravityChance > 0;

    [ModdedNumberOption("Gravity Field Radius", 0.5f, 10f, 0.5f, suffixType: MiraNumberSuffixes.None)]
    public float FieldRadius { get; set; } = 2f;
}