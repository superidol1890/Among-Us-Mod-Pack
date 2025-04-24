using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// The event that is invoked when the player tries to select another player to vote.
/// </summary>
public class MeetingSelectEvent : MiraEvent
{
    /// <summary>
    /// Gets the instance of the voter's vote data.
    /// </summary>
    public PlayerVoteData VoteData { get; }

    /// <summary>
    /// Gets the player id of the target.
    /// </summary>
    public int TargetId { get; }

    /// <summary>
    /// Gets the player info of the target.
    /// </summary>
    public NetworkedPlayerInfo TargetPlayerInfo { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the player is allowed to select the target.
    /// </summary>
    public bool AllowSelect { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MeetingSelectEvent"/> class.
    /// </summary>
    /// <param name="playerVoteData">The voter's data.</param>
    /// <param name="targetId">The target's playerId.</param>
    /// <param name="allowSelect">>Whether the player is allowed to select the target.</param>
    public MeetingSelectEvent(PlayerVoteData playerVoteData, int targetId, bool allowSelect)
    {
        VoteData = playerVoteData;
        TargetId = targetId;
        TargetPlayerInfo = GameData.Instance.GetPlayerById((byte)targetId);
        AllowSelect = allowSelect;
    }
}
