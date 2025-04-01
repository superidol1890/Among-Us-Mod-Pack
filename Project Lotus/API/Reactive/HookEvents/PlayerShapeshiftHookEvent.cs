namespace Lotus.API.Reactive.HookEvents;

public class PlayerShapeshiftHookEvent : IHookEvent
{
    public PlayerControl Player;
    public NetworkedPlayerInfo Target;
    public bool Reverted;

    public PlayerShapeshiftHookEvent(PlayerControl player, NetworkedPlayerInfo target, bool reverted)
    {
        Player = player;
        Target = target;
        Reverted = reverted;
    }
}