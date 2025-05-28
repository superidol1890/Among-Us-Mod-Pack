namespace Lotus.API.Reactive.HookEvents;

public class PlayerPhantomHookEvent(PlayerControl player, bool isInvisible): IHookEvent
{
    public PlayerControl Player = player;
    public bool IsInvisible = isInvisible;
}