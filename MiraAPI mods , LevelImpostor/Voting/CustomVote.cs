namespace MiraAPI.Voting;

/// <summary>
/// Represents a custom vote.
/// </summary>
/// <param name="Voter">The player that voted.</param>
/// <param name="Suspect">The suspect being voted for.</param>
public record struct CustomVote(byte Voter, byte Suspect);
