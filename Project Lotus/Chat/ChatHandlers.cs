using Lotus.Chat.Commands;
using VentLib.Utilities;

namespace Lotus.Chat;

public class ChatHandlers : CommandTranslations
{
    public static ChatHandler NotPermitted(string? message = null)
    {
        return ChatHandler.Of(message ?? NotPermittedText, ModConstants.Palette.KillingColor.Colorize(NotPermittedTitle)).LeftAlign();
    }

    public static ChatHandler InvalidCmdUsage(string? message = null)
    {
        return ChatHandler.Of(message ?? InvalidUsage, ModConstants.Palette.InvalidUsage.Colorize(InvalidUsage)).LeftAlign();
    }
}