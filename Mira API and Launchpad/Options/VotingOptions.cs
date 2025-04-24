using LaunchpadReloaded.Features.Voting;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using System;

namespace LaunchpadReloaded.Options;

public class VotingOptions : AbstractOptionGroup
{
    public override string GroupName => "Voting Type";

    public override Func<bool> GroupVisible => CustomGameModeManager.IsDefault;

    [ModdedEnumOption("Voting Type", typeof(VotingTypes))]
    public VotingTypes VotingType { get; set; } = VotingTypes.Classic;

    public ModdedNumberOption MaxVotes { get; } = new("Max Votes", 3, 2, 5, 1, MiraNumberSuffixes.None)
    {
        Visible = VotingTypesManager.CanVoteMultiple
    };

    public ModdedToggleOption AllowVotingForSamePerson { get; } = new("Allow Multiple Votes on Same Player", false)
    {
        Visible = VotingTypesManager.CanVoteMultiple
    };

    public ModdedToggleOption AllowConfirmingVotes { get; } = new("Allow Confirming Votes", false)
    {
        Visible = () => !VotingTypesManager.CanVoteMultiple()
    };

    public ModdedToggleOption HideVotingIcons { get; } = new("Hide Voting Icons", false)
    {
        Visible = () => VotingTypesManager.UseChance() || OptionGroupSingleton<VotingOptions>.Instance.ShowPercentages.Value
    };

    public ModdedToggleOption ShowPercentages { get; } = new("Show Percentages", false)
    {
        Visible = () => !VotingTypesManager.UseChance()
    };

}