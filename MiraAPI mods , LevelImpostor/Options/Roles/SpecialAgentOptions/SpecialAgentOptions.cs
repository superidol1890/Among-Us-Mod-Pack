using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles.SpecialAgentOptions
{
    public class SpecialAgentOptions : AbstractOptionGroup<SpecialAgent>
    {
        public override string GroupName => "Special Agent";

        [ModdedNumberOption("Assign Mission Cooldown", min: 10, max: 30)]
        public float AssignCooldown { get; set; } = 20f;

        [ModdedNumberOption("Assign Mission Max Uses", min: 1, max: 3)]
        public float AssignMaxUses { get; set; } = 3f;

        [ModdedToggleOption("Enable Target Camera Tracking")]
        public bool TargetCameraTracking { get; set; } = true;

        [ModdedNumberOption("Camera Tracking Duration", min: 5, max: 15)]
        public float CameraTrackingDuration { get; set; } = 10f;

        [ModdedNumberOption("Required Missions to Win", min: 1, max: 5)]
        public float RequiredMissionsToWin { get; set; } = 3f;

        [ModdedToggleOption("The camera should shake when the timer is close to the target")]
        public bool ShouldShakeCamera { get; set; } = false;
    }
}
