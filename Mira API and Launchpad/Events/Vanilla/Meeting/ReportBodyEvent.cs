namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when a body is reported.
/// This is invoked in CmdReportBody.
/// </summary>
public class ReportBodyEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the player that is reporting the body.
    /// </summary>
    public PlayerControl Reporter { get; }

    /// <summary>
    /// Gets the player that is being reported. Will be null for emergency meeting.
    /// </summary>
    public NetworkedPlayerInfo? Target { get; }

    /// <summary>
    /// Gets the body that is being reported. Will be null for emergency meeting.
    /// </summary>
    public DeadBody? Body { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportBodyEvent"/> class.
    /// </summary>
    /// <param name="reporter">The player who reported the body.</param>
    /// <param name="target">The player being reported.</param>
    /// <param name="body">The body being reported.</param>
    public ReportBodyEvent(PlayerControl reporter, NetworkedPlayerInfo? target, DeadBody? body)
    {
        Reporter = reporter;
        Target = target;
        Body = body;
    }
}
