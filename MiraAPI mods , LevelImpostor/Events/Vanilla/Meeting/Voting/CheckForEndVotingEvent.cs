namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Checks if voting is complete. If canceled, the default end voting logic will be skipped.
/// </summary>
public class CheckForEndVotingEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets a value indicating whether default voting logic determines that voting is complete.
    /// </summary>
    public bool IsVotingComplete { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the voting should be forced to end, regardless of the default logic.
    /// </summary>
    public bool ForceEndVoting { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckForEndVotingEvent"/> class.
    /// </summary>
    /// <param name="isVotingComplete">>True if voting is complete, false otherwise.</param>
    public CheckForEndVotingEvent(bool isVotingComplete)
    {
        IsVotingComplete = isVotingComplete;
    }
}
