using LaunchpadReloaded.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Crewmate;

public class MedicOptions : AbstractOptionGroup<MedicRole>
{
    public override string GroupName => "Medic";

    [ModdedToggleOption("Only Allow Reviving in MedBay/Laboratory")]
    public bool OnlyAllowInMedbay { get; set; } = false;

    [ModdedToggleOption("Can Drag Bodies")]
    public bool DragBodies { get; set; } = false;

    [ModdedNumberOption("Max Revives", 1, 9)]
    public float MaxRevives { get; set; } = 2;

    [ModdedNumberOption("Revive Cooldown", 1, 50, 2, MiraNumberSuffixes.Seconds)]
    public float ReviveCooldown { get; set; } = 20;
}