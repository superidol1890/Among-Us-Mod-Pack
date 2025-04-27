using HarmonyLib;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    public static class CanMove
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result)
        {
            // innersloth naming sucks, displayed is actually displaying
            __result = (__result || HudManager.Instance.IsIntroDisplayed) && !Minigame.Instance;
        }
    }
}