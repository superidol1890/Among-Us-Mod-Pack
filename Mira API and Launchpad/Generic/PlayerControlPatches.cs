using HarmonyLib;
using LaunchpadReloaded.Components;
using LaunchpadReloaded.Features;
using LaunchpadReloaded.Features.Managers;
using LaunchpadReloaded.Modifiers;
using LaunchpadReloaded.Networking.Color;
using LaunchpadReloaded.Roles.Impostor;
using LaunchpadReloaded.Utilities;
using Reactor.Networking.Rpc;
using System.Linq;
using MiraAPI.Modifiers;
using UnityEngine;
using Action = System.Action;

namespace LaunchpadReloaded.Patches.Generic;

[HarmonyPatch(typeof(PlayerControl))]
public static class PlayerControlPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerControl.Shapeshift))]
    public static bool ShapeshiftPatch(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        __instance.waitingForShapeshiftResponse = false;
        if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup)
        {
            if (__instance.AmOwner && __instance.Data.Role is ShapeshifterRole)
            {
                DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(__instance.Data.Role.Ability);
                __instance.Data.Role.SetCooldown();
            }

            return false;
        }
        var targetPlayerInfo = targetPlayer.Data;
        NetworkedPlayerInfo.PlayerOutfit newOutfit;
        if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
        {
            newOutfit = __instance.Data.Outfits[PlayerOutfitType.Default];
        }
        else
        {
            newOutfit = targetPlayer.Data.Outfits[PlayerOutfitType.Default];
        }

        var changeOutfit = new Action(() =>
        {
            if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
            {
                __instance.RawSetOutfit(newOutfit, PlayerOutfitType.Default);

                var playerModComponent = __instance.GetModifierComponent();

                if (playerModComponent != null
                    && __instance.gameObject.TryGetComponent<ModifierStorageComponent>(out var modStorage)
                    && modStorage.Modifiers != null)
                {
                    __instance.ClearModifiers();

                    foreach (var mod in modStorage.Modifiers)
                    {
                        playerModComponent.AddModifier(mod);
                    }

                    modStorage.Modifiers.Clear();
                }

                __instance.shapeshiftTargetPlayerId = -1;

                if (__instance.AmOwner && __instance.Data.Role is ShapeshifterRole)
                {
                    DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(__instance.Data.Role.Ability);
                    return;
                }
            }
            else
            {
                __instance.RawSetOutfit(newOutfit, PlayerOutfitType.Shapeshifted);
                __instance.shapeshiftTargetPlayerId = (int)targetPlayer.PlayerId;

                var playerModifiers = __instance.GetModifierComponent()?.ActiveModifiers.ToList();
                var targetModifiers = targetPlayer.GetModifierComponent()?.ActiveModifiers.ToList();
                var playerModComponent = __instance.GetModifierComponent();
                var modifierStorage = __instance.gameObject.AddComponent<ModifierStorageComponent>();

                modifierStorage.Modifiers = playerModifiers;

                if (playerModifiers != null && targetModifiers != null && playerModComponent != null)
                {
                    __instance.ClearModifiers();

                    foreach (var mod in targetModifiers)
                    {
                        if (mod is RevealedModifier)
                        {
                            return;
                        }

                        playerModComponent.AddModifier(mod);
                    }
                }

                if (__instance.AmOwner && __instance.Data.Role is ShapeshifterRole)
                {
                    DestroyableSingleton<HudManager>.Instance.AbilityButton.OverrideText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShapeshiftAbilityUndo, Il2CppSystem.Array.Empty<Il2CppSystem.Object>()));
                }
            }

            __instance.GetTagManager()?.UpdatePosition();
        });


        if (animate)
        {
            __instance.shapeshifting = true;
            __instance.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            if (__instance.AmOwner && !Minigame.Instance)
            {
                PlayerControl.HideCursorTemporarily();
            }
            var roleEffectAnimation = Object.Instantiate(DestroyableSingleton<RoleManager>.Instance.shapeshiftAnim, __instance.gameObject.transform);
            roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(__instance.AmOwner);
            roleEffectAnimation.SetMaterialColor(__instance.Data.Outfits[PlayerOutfitType.Default].ColorId);
            if (__instance.cosmetics.FlipX)
            {
                roleEffectAnimation.transform.position -= new Vector3(0.14f, 0f, 0f);
            }

            roleEffectAnimation.MidAnimCB = new Action(() =>
            {
                changeOutfit();
                __instance.cosmetics.SetScale(__instance.MyPhysics.Animations.DefaultPlayerScale, __instance.defaultCosmeticsScale);

                if (__instance.Data.Role is ShapeshifterRole role)
                {
                    role.SetEvidence();
                }
            });

            var shapeshiftScale = __instance.MyPhysics.Animations.ShapeshiftScale;
            if (AprilFoolsMode.ShouldLongAround())
            {
                __instance.cosmetics.ShowLongModeParts(false);
                __instance.cosmetics.SetHatVisorVisible(false);
            }
            __instance.StartCoroutine(__instance.ScalePlayer(shapeshiftScale, 0.25f));
            roleEffectAnimation.Play(__instance, new Action(() =>
            {
                __instance.shapeshifting = false;
                if (AprilFoolsMode.ShouldLongAround())
                {
                    __instance.cosmetics.ShowLongModeParts(true);
                    __instance.cosmetics.SetHatVisorVisible(true);
                }
            }), PlayerControl.LocalPlayer.cosmetics.FlipX, RoleEffectAnimation.SoundType.Local, 0f, true, 0f);
            return false;
        }
        changeOutfit();

        return false;
    }
    /// <summary>
    /// Disable kill timer while janitor is dragging
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.IsKillTimerEnabled), MethodType.Getter)]
    public static void GetKillTimerEnabledPostfix(PlayerControl __instance, ref bool __result)
    {
        switch (__instance.Data.Role)
        {
            case JanitorRole:
                __result = __result && !__instance.HasModifier<DragBodyModifier>();
                break;
        }
    }


    /// <summary>
    /// Disable kill timer while janitor is dragging
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.CanMove), MethodType.Getter)]
    public static void CanMovePatch(PlayerControl __instance, ref bool __result)
    {
        if (NotepadHud.Instance?.Notepad.active == true)
        {
            __result = false;
        }
    }


    /// <summary>
    /// Use Custom check color RPC
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerControl.CmdCheckColor))]
    public static bool CheckColorPatch(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            Rpc<CustomCmdCheckColor>.Instance.Handle(__instance, new CustomColorData(bodyColor, (byte)GradientManager.LocalGradientId));
            return false;
        }

        Rpc<CustomCmdCheckColor>.Instance.SendTo(AmongUsClient.Instance.HostId, new CustomColorData(bodyColor, (byte)GradientManager.LocalGradientId));
        return false;
    }

    /// <summary>
    /// Set gradient 
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.Start))]
    public static void StartPostfix(PlayerControl __instance)
    {
        __instance.gameObject.AddComponent<PlayerGradientData>();
        __instance.gameObject.AddComponent<PlayerTagManager>();
    }

    /// <summary>
    /// Update gradient
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.SetPlayerMaterialColors))]
    public static void SetPlayerMaterialColorsPostfix(PlayerControl __instance, [HarmonyArgument(0)] Renderer renderer)
    {
        var playerGradient = __instance.GetComponent<PlayerGradientData>();
        if (playerGradient)
        {
            renderer.GetComponent<GradientColorComponent>().SetColor(__instance.Data.DefaultOutfit.ColorId, playerGradient.GradientColor);
        }
    }
}