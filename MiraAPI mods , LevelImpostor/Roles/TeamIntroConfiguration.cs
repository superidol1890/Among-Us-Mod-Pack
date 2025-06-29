using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Used to configure the TEAM part of the intro cutscene.
/// </summary>
/// <param name="IntroTeamColor">The color to show during the "Team" part of the intro cutscene.</param>
/// <param name="IntroTeamTitle">The title text to show during the "Team" part of the intro cutscene.</param>
/// <param name="IntroTeamDescription">The description text to show during the "Team" part of the intro cutscene.</param>
public record struct TeamIntroConfiguration(Color IntroTeamColor, string IntroTeamTitle, string IntroTeamDescription)
{
    /// <summary>
    /// Gets the default configuration for the "Neutral" team.
    /// </summary>
    public static TeamIntroConfiguration Neutral { get; } = new(
        Color.gray,
        "NEUTRAL",
        "You are Neutral. You do not have a team.");
}
