using LaunchpadReloaded.Roles.Outcast;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadReloaded.Options.Roles.Neutral;

public class ExecutionerOptions : AbstractOptionGroup<ExecutionerRole>
{
    public override string GroupName => "Executioner";

    [ModdedToggleOption("Can Call Meeting")]
    public bool CanCallMeeting { get; set; } = false;

    [ModdedEnumOption("On Target Death, Executioner Becomes", typeof(ExecutionerBecomes))]
    public ExecutionerBecomes TargetDeathNewRole { get; set; } = ExecutionerBecomes.Jester;

    public enum ExecutionerBecomes
    {
        Crewmate,
        Jester,
    }
}