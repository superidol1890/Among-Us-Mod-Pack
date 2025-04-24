using System;

namespace MiraAPI.Events;

/// <summary>
/// Register an event.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RegisterEventAttribute : Attribute
{
    /// <summary>
    /// Gets the priority of the event. Lower values are called first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEventAttribute"/> class.
    /// </summary>
    /// <param name="priority">The priority of the event. Lower values are called first.</param>
    public RegisterEventAttribute(int priority = 0)
    {
        Priority = priority;
    }
}
