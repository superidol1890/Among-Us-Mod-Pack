namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when the meeting has ended.
/// </summary>
public class EndMeetingEvent(MeetingHud meetingHud) : MiraEvent
{
    /// <summary>
    /// Gets the MeetingHud instance.
    /// </summary>
    public MeetingHud MeetingHud { get; } = meetingHud;
}
