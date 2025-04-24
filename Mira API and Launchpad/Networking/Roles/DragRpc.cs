using LaunchpadReloaded.Buttons;
using LaunchpadReloaded.Modifiers;
using LaunchpadReloaded.Roles.Crewmate;
using LaunchpadReloaded.Roles.Impostor;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;

namespace LaunchpadReloaded.Networking.Roles;

public static class DragRpc
{
    [MethodRpc((uint)LaunchpadRpc.StartDrag)]
    public static void RpcStartDragging(this PlayerControl playerControl, byte bodyId)
    {
        var role = playerControl.Data.Role;
        if (role is not JanitorRole && role is not MedicRole)
        {
            playerControl.KickForCheating();
            return;
        }

        playerControl.GetModifierComponent()?.AddModifier(new DragBodyModifier(bodyId));

        if (playerControl.AmOwner)
        {
            CustomButtonSingleton<DragButton>.Instance.SetDrop();
        }
    }

    [MethodRpc((uint)LaunchpadRpc.StopDrag)]
    public static void RpcStopDragging(this PlayerControl playerControl)
    {
        playerControl.GetModifierComponent()?.RemoveModifier<DragBodyModifier>();

        if (playerControl.AmOwner)
        {
            CustomButtonSingleton<DragButton>.Instance.SetDrag();
        }
    }

}