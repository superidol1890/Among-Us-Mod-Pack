namespace Lotus.Roles.Interfaces;

/// <summary>
/// A role that inherits this class will send text when the player uses the /info command.
/// The /info command allows the user to resend any messages that were sent at the start of the meeting.
/// Don't resend messages to everyone, as that is spam, and a dead giveaway of the player's role.
/// </summary>
public interface IInfoResender
{
    /// <summary>
    /// This function is called when MyPlayer runs /info.
    /// </summary>
    public void ResendMessages();
}