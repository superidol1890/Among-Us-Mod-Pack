using Lotus.API.Vanilla.Meetings;

namespace Lotus.API.Reactive.HookEvents;

public class MeetingHookEvent : IHookEvent
{
    public PlayerControl Caller;
    public NetworkedPlayerInfo? Reported;
    public MeetingDelegate Delegate;

    public MeetingHookEvent(PlayerControl caller, NetworkedPlayerInfo? reporter, MeetingDelegate meetingDelegate)
    {
        Caller = caller;
        Reported = reporter;
        Delegate = meetingDelegate;
    }
}