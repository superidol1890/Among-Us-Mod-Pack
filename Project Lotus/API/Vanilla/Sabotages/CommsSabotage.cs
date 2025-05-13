using Hazel;
using Il2CppSystem;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Operations;
using VentLib.Utilities.Optionals;

namespace Lotus.API.Vanilla.Sabotages;

public class CommsSabotage : ISabotage
{
    private UnityOptional<PlayerControl> caller;

    public CommsSabotage(PlayerControl? player = null)
    {
        caller = player == null ? UnityOptional<PlayerControl>.Null() : UnityOptional<PlayerControl>.NonNull(player);
    }

    public SabotageType SabotageType() => Sabotages.SabotageType.Communications;

    public bool Fix(PlayerControl? fixer = null)
    {

        ActionHandle handle = ActionHandle.NoInit();
        RoleOperations.Current.TriggerForAll(LotusActionType.SabotageFixed, fixer == null ? PlayerControl.LocalPlayer : fixer, handle, this);
        if (handle.IsCanceled) return false;
        fixer = fixer == null ? PlayerControl.LocalPlayer : fixer;


        if (!ShipStatus.Instance.TryGetSystem(SabotageType().ToSystemType(), out ISystemType? systemInstance)) return false;
        if (systemInstance!.TryCast<HudOverrideSystemType>() != null)
        {
            HudOverrideSystemType hudOverrideSystemType = systemInstance.Cast<HudOverrideSystemType>();
            hudOverrideSystemType.IsActive = false;
            hudOverrideSystemType.IsDirty = true;

        }
        else if (systemInstance.TryCast<HqHudSystemType>() != null) // Mira has a special communications which requires two people
        {
            HqHudSystemType miraComms = systemInstance.Cast<HqHudSystemType>(); // Get mira comm instance
            miraComms.CompletedConsoles.Add(0);
            miraComms.CompletedConsoles.Add(1);
            miraComms.ActiveConsoles.Add(new Tuple<byte, byte>(fixer.PlayerId, 0));
            miraComms.ActiveConsoles.Add(new Tuple<byte, byte>(fixer.PlayerId, 1));
            miraComms.IsDirty = true;
        }

        SabotagePatch.CurrentSabotage = null;
        Hooks.SabotageHooks.SabotageFixedHook.Propagate(new SabotageFixHookEvent(fixer, this));
        return true;
    }

    public Optional<PlayerControl> Caller() => caller;

    public void CallSabotage(PlayerControl sabotageCaller)
    {
        ActionHandle handle = ActionHandle.NoInit();
        RoleOperations.Current.TriggerForAll(LotusActionType.SabotageStarted, sabotageCaller, handle, this);
        if (handle.IsCanceled) return;

        ShipStatus.Instance.UpdateSystem(SabotageType().ToSystemType(), sabotageCaller, 128);
        caller.OrElseSet(() => sabotageCaller);
        SabotagePatch.CurrentSabotage = this;
    }

    public override string ToString() => $"OxygenSabotage(Caller={caller})";
}