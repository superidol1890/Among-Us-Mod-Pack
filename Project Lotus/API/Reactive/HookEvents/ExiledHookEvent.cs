using System.Collections.Generic;

namespace Lotus.API.Reactive.HookEvents;

public class ExiledHookEvent : IHookEvent
{
    public NetworkedPlayerInfo ExiledPlayer;
    public List<byte> Voters;

    public ExiledHookEvent(NetworkedPlayerInfo exiledPlayer, List<byte> voters)
    {
        ExiledPlayer = exiledPlayer;
        Voters = voters;
    }
}