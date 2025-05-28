using System;

namespace Lotus.GameModes;

[Flags]
public enum BlockableGameAction
{
    Nothing = 0,
    ReportBody = 1 << 0,
    CallMeeting = 1 << 1,
    KillPlayers = 1 << 2,
    CallSabotage = 1 << 3,
    CloseDoors = 1 << 4,
    EnterVent = 1 << 5
}