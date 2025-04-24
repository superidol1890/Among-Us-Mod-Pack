using MiraAPI.Hud;

namespace MiraAPI.Events.Mira;

/// <summary>
/// Invoked when a Mira Button click is cancelled. Do not use for vanilla buttons.
/// </summary>
/// <typeparam name="T">The Mira Button type.</typeparam>
public sealed class MiraButtonCancelledEvent<T> : MiraEvent where T : CustomActionButton
{
    /// <summary>
    /// Gets Mira Button whose click was cancelled.
    /// </summary>
    public T Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiraButtonCancelledEvent{T}"/> class.
    /// </summary>
    /// <param name="button">The Mira Button whose click was cancelled.</param>
    public MiraButtonCancelledEvent(T button)
    {
        Button = button;
    }
}

/// <summary>
/// Invoked when a Mira Button click is cancelled. Do not use for vanilla buttons.
/// </summary>
public sealed class MiraButtonCancelledEvent : MiraEvent
{
    /// <summary>
    /// Gets Mira Button whose click was cancelled.
    /// </summary>
    public CustomActionButton Button { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiraButtonCancelledEvent"/> class.
    /// </summary>
    /// <param name="button">The Mira Button whose click was cancelled.</param>
    public MiraButtonCancelledEvent(CustomActionButton button)
    {
        Button = button;
    }
}
