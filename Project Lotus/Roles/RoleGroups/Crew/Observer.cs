using Lotus.API;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Crew;

public class Observer : Crewmate
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Observer));
    private bool slowlyGainsVision;
    private float visionGain;
    private bool overrideStartingVision;
    private float startingVision;
    private float totalVisionMod;

    private float currentVisionMod;
    private bool sabotageImmunity;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        currentVisionMod = overrideStartingVision ? startingVision : AUSettings.CrewLightMod();
    }

    protected override void OnTaskComplete(Optional<NormalPlayerTask> _)
    {
        if (slowlyGainsVision)
            currentVisionMod = Mathf.Clamp(currentVisionMod + visionGain, 0, totalVisionMod);
        if (HasAllTasksComplete)
            currentVisionMod = totalVisionMod;
        SyncOptions();
    }

    [RoleAction(LotusActionType.SabotageStarted, ActionFlag.GlobalDetector)]
    [RoleAction(LotusActionType.SabotageFixed, ActionFlag.GlobalDetector)]
    private void AdjustSabotageVision(ActionHandle handle)
    {
        log.Trace($"Fixing Player Vision (HasAllTasksComplete = {HasAllTasksComplete}, SabotageImmune = {sabotageImmunity})", "Observer");
        Async.Schedule(SyncOptions, handle.ActionType is LotusActionType.SabotageStarted ? 4f : 0.2f);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Slowly Gains Vision", Translations.Options.SlowlyGainsVision)
                .BindBool(v => slowlyGainsVision = v)
                .AddOnOffValues(false)
                .ShowSubOptionPredicate(v => (bool)v)
                .SubOption(sub2 => sub2
                    .KeyName("Vision Gain On Task Complete", Translations.Options.VisionGain)
                    .BindFloat(v => visionGain = v)
                    .AddFloatRange(0.05f, 1, 0.05f, 2, "x").Build())
                .Build())
            .SubOption(sub => sub
                .KeyName("Override Starting Vision", Translations.Options.OverrideVision)
                .BindBool(v => overrideStartingVision = v)
                .ShowSubOptionPredicate(v => (bool)v)
                .AddOnOffValues(false)
                .SubOption(sub2 => sub2
                    .KeyName("Starting Vision Modifier", Translations.Options.StartingVision)
                    .BindFloat(v => startingVision = v)
                    .AddFloatRange(0.25f, 2, 0.25f, 0, "x").Build())
                .Build())
            .SubOption(sub => sub
                .KeyName("Finished Tasks Vision", Translations.Options.FinalVision)
                .BindFloat(v => totalVisionMod = v)
                .AddFloatRange(0.25f, 5f, 0.25f, 8, "x").Build())
            .SubOption(sub => sub
                .KeyName("Lights Immunity If Tasks Finished", Translations.Options.LightsImmunity)
                .BindBool(v => sabotageImmunity = v)
                .AddOnOffValues().Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.93f, 0.9f, 0.75f))
            .OptionOverride(Override.CrewLightMod, () => currentVisionMod)
            .OptionOverride(Override.CrewLightMod, () => currentVisionMod * 5,
                () => sabotageImmunity && HasAllTasksComplete && SabotagePatch.CurrentSabotage != null && SabotagePatch.CurrentSabotage.SabotageType() is SabotageType.Lights);

    [Localized(nameof(Observer))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(SlowlyGainsVision))]
            public static string SlowlyGainsVision = "Slowly Gains Vision";

            [Localized(nameof(VisionGain))]
            public static string VisionGain = "Vision Gain  on Task Complete";

            [Localized(nameof(OverrideVision))]
            public static string OverrideVision = "Override Starting Vision";

            [Localized(nameof(StartingVision))]
            public static string StartingVision = "Starting Vision Modifier";

            [Localized(nameof(FinalVision))]
            public static string FinalVision = "Finished Tasks Vision";

            [Localized(nameof(LightsImmunity))]
            public static string LightsImmunity = "Lights Immunity if Tasks Finished";
        }
    }
}