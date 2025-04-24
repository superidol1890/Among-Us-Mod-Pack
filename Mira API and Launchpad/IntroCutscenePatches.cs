using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Roles;
using MiraAPI.Utilities;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(IntroCutscene))]
public static class IntroCutscenePatches
{
    /*
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    public static void BeginImpostorPatch(IntroCutscene __instance)
    {
        if (CustomGameModeManager.ActiveMode != null && CustomGameModeManager.ActiveMode.ShowCustomRoleScreen())
        {
            var mode = CustomGameModeManager.ActiveMode;
            __instance.TeamTitle.text = $"<size=70%>{mode.Name}</size>\n<size=20%>{mode.Description}</size>";
        }
    }*/

    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.CoBegin))]
    public static void IntroBeginPatch(IntroCutscene __instance)
    {
        var @event = new IntroBeginEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
    public static bool BeginPrefix(IntroCutscene __instance, [HarmonyArgument(0)] ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        return PlayerControl.LocalPlayer.Data.Role is not ICustomRole customRole || customRole.SetupIntroTeam(__instance, ref yourTeam);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
    public static void BeginPostfix(IntroCutscene __instance)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole customRole)
        {
            return;
        }

        if (customRole.IntroConfiguration is { } introConfig)
        {
            __instance.BackgroundBar.material.SetColor(ShaderID.Color, introConfig.IntroTeamColor);
            __instance.TeamTitle.color = introConfig.IntroTeamColor;
            __instance.TeamTitle.text = introConfig.IntroTeamTitle;
            __instance.ImpostorText.text = introConfig.IntroTeamDescription;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.OnDestroy))]
    public static void GameBeginPatch(IntroCutscene __instance)
    {
        MiraEventManager.InvokeEvent(new IntroEndEvent(__instance));
        MiraEventManager.InvokeEvent(new RoundStartEvent(true));
    }
}
