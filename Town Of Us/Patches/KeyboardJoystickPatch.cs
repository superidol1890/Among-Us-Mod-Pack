using HarmonyLib;

namespace TownOfUs
{
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.HandleHud))]
    public class KeyboardJoystickPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (HudManager.Instance != null && HudManager.Instance.ImpostorVentButton != null && HudManager.Instance.ImpostorVentButton.isActiveAndEnabled && ConsoleJoystick.player.GetButtonDown(50))
                HudManager.Instance.ImpostorVentButton.DoClick();
        }
    }
}
