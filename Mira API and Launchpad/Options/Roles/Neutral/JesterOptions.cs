using LaunchpadReloaded.Roles.Outcast;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadReloaded.Options.Roles.Neutral;

public class JesterOptions : AbstractOptionGroup<JesterRole>
{
    public override string GroupName => "Jester";

    [ModdedToggleOption("Can Use Vents")]
    public bool CanUseVents { get; set; } = true;
}