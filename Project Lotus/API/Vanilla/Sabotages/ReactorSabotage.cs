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

public class ReactorSabotage : ISabotage
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ReactorSabotage));

    private UnityOptional<PlayerControl> caller;

    public ReactorSabotage(PlayerControl? player = null)
    {
        caller = player == null ? UnityOptional<PlayerControl>.Null() : UnityOptional<PlayerControl>.NonNull(player);
    }

    public SabotageType SabotageType() => Sabotages.SabotageType.Reactor;

    public bool Fix(PlayerControl? fixer = null)
    {
        ActionHandle handle = ActionHandle.NoInit();
        RoleOperations.Current.TriggerForAll(LotusActionType.SabotageFixed, fixer == null ? PlayerControl.LocalPlayer : fixer, handle, this);
        if (handle.IsCanceled) return false;

        if (!ShipStatus.Instance.TryGetSystem(SabotageType().ToSystemType(), out ISystemType? systemInstance)) return false;
        log.Info($"System Instance: {systemInstance}");
        ReactorSystemType? reactor = systemInstance!.TryCast<ReactorSystemType>();
        if (reactor == null)
        {
            log.Warn($"Error Fixing Reactor Sabotage. Invalid System Cast from {SabotageType()}.");
            return false;
        }

        reactor.Countdown = 10000f;
        reactor.IsDirty = true;
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

    public override string ToString() => $"ReactorSabotage(Caller={caller})";
}