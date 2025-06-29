using LaunchpadReloaded.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Impostor;

public class JanitorOptions : AbstractOptionGroup<JanitorRole>
{
    public override string GroupName => "Janitor";

    [ModdedNumberOption("Hide Bodies Cooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float HideCooldown { get; set; } = 5f;

    [ModdedNumberOption("Drag Body Speed", 0.5f, 2.5f, 0.25f, MiraNumberSuffixes.None)]
    public float DragSpeed { get; set; } = 1.75f;

    [ModdedNumberOption("Hide Bodies Uses", 0, 10, zeroInfinity: true)]
    public float HideUses { get; set; } = 3;

    [ModdedToggleOption("Clean Instead Of Hide")]
    public bool CleanInsteadOfHide { get; set; } = false;
}