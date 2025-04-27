using HarmonyLib;

namespace TownOfUs.RainbowMod
{
    [HarmonyPatch(typeof(AmongUs.Data.Player.PlayerData), nameof(AmongUs.Data.Player.PlayerData.FileName), MethodType.Getter)]
    public class SaveManagerPatch
    {
        public static void Postfix(ref string __result)
        {
            __result += "_TOU";
        }
    }
    [HarmonyPatch(typeof(AmongUs.Data.Legacy.LegacySaveManager), nameof(AmongUs.Data.Legacy.LegacySaveManager.GetPrefsName))]
    public class LegacySaveManagerPatch
    {
        public static void Postfix(ref string __result)
        {
            __result += "_TOU";
        }
    }

    [HarmonyPatch(typeof(AmongUs.Data.Settings.SettingsData), nameof(AmongUs.Data.Settings.SettingsData.FileName), MethodType.Getter)]
    public class SettingsFilePatch
    {
        public static void Postfix(ref string __result)
        {
            __result += "_TOU";
        }
    }
}
