using System;

namespace Lotus.GameModes;

[Flags]
public enum GameModeFlags
{
    None = 0,
    AllowChatDuringGame = 1 << 0
}