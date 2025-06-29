using System;

namespace MiraAPI.Events;

/// <summary>
/// Event handle to use when unregistering an event.
/// </summary>
public class MiraEventHandle
{
    /// <summary>
    /// Gets the type of event.
    /// </summary>
    public Type EventType { get; }

    internal MiraEventWrapper EventWrapper { get; }

    internal MiraEventHandle(Type type, MiraEventWrapper eventWrapper)
    {
        EventType = type;
        EventWrapper = eventWrapper;
    }
}
