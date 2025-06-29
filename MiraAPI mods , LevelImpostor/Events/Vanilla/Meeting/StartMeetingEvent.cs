namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when a meeting is called. This event is not cancelable.
/// This event is called after Mira resets votes, so if you plan on adding votes to a specific player, do it with this event.
/// </summary>
public class StartMeetingEvent : MiraEvent
{
    /// <summary>
    /// Gets the MeetingHud instance.
    /// </summary>
    public MeetingHud MeetingHud { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StartMeetingEvent"/> class.
    /// </summary>
    /// <param name="meetingHud">The MeetingHud instance.</param>
    public StartMeetingEvent(MeetingHud meetingHud)
    {
        MeetingHud = meetingHud;
    }
}
