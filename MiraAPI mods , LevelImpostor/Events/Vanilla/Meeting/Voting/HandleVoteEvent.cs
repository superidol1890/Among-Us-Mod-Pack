using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// The event that is invoked when Mira handles a vote. This is invoked before Mira's behaviour, so be cautious.
/// If you intend on adding custom vote behaviour, cancel the event and do so.
/// If you are NOT canceling the event, keep in mind that Mira automatically removes a vote and adds the suspect to the VotedPlayers list after this event is invoked.
/// </summary>
public class HandleVoteEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of the voter's vote data.
    /// </summary>
    public PlayerVoteData VoteData { get; }

    /// <summary>
    /// Gets the player who voted.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the player id of the target.
    /// </summary>
    public byte TargetId { get; }

    /// <summary>
    /// Gets a value indicating whether to prevent the vote from commencing.
    /// </summary>
    public bool PreventVote { get; }

    /// <summary>
    /// Gets the player info of the target.
    /// </summary>
    public NetworkedPlayerInfo TargetPlayerInfo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandleVoteEvent"/> class.
    /// </summary>
    /// <param name="playerVoteData">The voter's data.</param>
    /// <param name="targetId">The target's playerId.</param>
    public HandleVoteEvent(PlayerVoteData playerVoteData, byte targetId)
    {
        VoteData = playerVoteData;
        Player = playerVoteData.Owner;
        TargetId = targetId;
        PreventVote = false;
        TargetPlayerInfo = GameData.Instance.GetPlayerById(targetId);
    }
}
