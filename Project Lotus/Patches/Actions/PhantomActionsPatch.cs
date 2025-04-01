using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using Lotus.API;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Impostors;
using Lotus.Roles;
using Lotus.Utilities;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Operations;
using Lotus.Roles.Overrides;

namespace Lotus.Patches.Actions;

// phantom is very broken right now...
// but there are some uses.
// we shouldn't have to fix your code
// pls do a proper fix innersloth

// code sampled from Endless Host Roles (Gurge44) (PhantomRolePatch)

public static class PhantomActionsPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(PhantomActionsPatch));

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
    public static bool CmdCheckVanishPrefix(PlayerControl __instance, float maxDuration)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            __instance.CheckVanish();
            return false;
        }

        __instance.SetRoleInvisibility(true);
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.CheckVanish, SendOption.Reliable, AmongUsClient.Instance.HostId);
        messageWriter.Write(maxDuration);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        return false;
    }

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
    public static bool CmdCheckAppearPrefix(PlayerControl __instance, bool shouldAnimate)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            __instance.CheckAppear(shouldAnimate);
            return false;
        }

        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.CheckAppear, SendOption.Reliable, AmongUsClient.Instance.HostId);
        messageWriter.Write(shouldAnimate);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        return false;
    }

    [QuickPrefix(typeof(PhantomRole), nameof(PhantomRole.UseAbility))]
    public static bool UseAbility(PhantomRole __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (!__instance.Player.IsAlive() || !__instance.Player.AmOwner || !__instance.Player.moveable || Minigame.Instance || __instance.IsCoolingDown || __instance.fading) return false;
        Func<RoleEffectAnimation, bool> roleEffectAnimation = x => x.effectType == RoleEffectAnimation.EffectType.Vanish_Charge;
        if (__instance.Player.currentRoleAnimations.Find(roleEffectAnimation) || __instance.Player.walkingToVent || __instance.Player.inMovingPlat) return false;
        if (__instance.isInvisible)
        {
            __instance.MakePlayerVisible(true, true);
            return false;
        }
        DestroyableSingleton<HudManager>.Instance.AbilityButton.SetSecondImage(__instance.Ability);
        DestroyableSingleton<HudManager>.Instance.AbilityButton.OverrideText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PhantomAbilityUndo,
            new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
        __instance.Player.CmdCheckVanish(AUSettings.PhantomDuration());
        return false;
    }

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.SetRoleInvisibility))]
    public static void OnChangeVisibility(PlayerControl __instance, bool isActive)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        log.Debug($"{__instance.name} {(isActive ? "is going invisible as Phantom." : "is appearing as Phantom.")}");
        IEnumerable<byte> alliedPlayerIds = Players.GetPlayers().Where(p => __instance.Relationship(p) is Relation.FullAllies).Where(__instance.PrimaryRole().Faction.CanSeeRole).Select(p => p.PlayerId);
        Vent farthestVent = Utils.GetFurthestVentFromPlayers();
        bool isImpFaction = __instance.PrimaryRole().Faction is ImpostorFaction;
        Players.GetAllPlayers().ForEach(p =>
        {
            if (__instance == p) return; // skip player going invis
            CustomRole role = p.PrimaryRole();
            if (role.RealRole.IsCrewmate() && isImpFaction) return; // they are crewmate, and we are imp so we are phantom for them
            if (alliedPlayerIds.Contains(p.PlayerId)) return; // if we are allied than continue
            if (isActive)
            {
                if (p.AmOwner)
                {
                    __instance.MyPhysics.StopAllCoroutines();
                    __instance.NetTransform.SnapTo(farthestVent.transform.position);
                    __instance.MyPhysics.StartCoroutine(__instance.MyPhysics.CoEnterVent(farthestVent.Id));
                }
                else
                {
                    Utils.TeleportDeferred(__instance.NetTransform, farthestVent.transform.position).Send(p.GetClientId());
                    RpcV3.Immediate(__instance.MyPhysics.NetId, RpcCalls.EnterVent).WritePacked(farthestVent.Id).Send(p.GetClientId());
                }
            }
            else
            {
                if (p.AmOwner)
                {
                    var pos = __instance.GetTruePosition();
                    __instance.MyPhysics.BootFromVent(farthestVent.Id);
                    __instance.NetTransform.SnapTo(pos);
                }
                else
                {
                    var pos = __instance.GetTruePosition();
                    RpcV3.Immediate(__instance.MyPhysics.NetId, RpcCalls.ExitVent).WritePacked(farthestVent.Id).Send(p.GetClientId());
                    Utils.TeleportDeferred(__instance.NetTransform, pos).Send(p.GetClientId());
                }
            }
        });
    }

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.CheckVanish))]
    public static bool InterceptVanish(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        PlayerControl phantom = __instance;
        log.Info($"CheckVanish - {phantom.GetNameWithRole()}");

        ActionHandle handle = ActionHandle.NoInit();
        RoleOperations.Current.Trigger(LotusActionType.Vanish, phantom, handle);
        if (handle.IsCanceled)
        {
            if (phantom.AmOwner)
            {
                DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(phantom.Data.Role.Ability);
                phantom.Data.Role.SetCooldown();
                return false;
            }

            RpcV3.Immediate(phantom.NetId, RpcCalls.SetRole).Write((ushort)RoleTypes.Phantom).Write(true).Send(phantom.GetClientId());
            phantom.RpcResetAbilityCooldown();

            Async.Schedule(() => phantom.SetKillCooldown(phantom.PrimaryRole().GetOverride(Override.KillCooldown)?.GetValue() as float? ?? AUSettings.KillCooldown()), 0.2f);

            return false;
        }

        return true;
    }

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.CheckAppear))]
    public static void InterceptAppear(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        PlayerControl phantom = __instance;
        log.Info($"CheckAppear - {phantom.GetNameWithRole()}");
    }
}