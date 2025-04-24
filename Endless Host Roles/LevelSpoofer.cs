using AmongUs.Data;
using HarmonyLib;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;

[BepInPlugin("com.levelspoofer.techiee", "Level Spoofer", "1.0")]
public class LevelSpooferPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony("Level Spoofer");
    public static ConfigEntry<string> LevelSpoofer;
    public override void Load()
    {
        LevelSpoofer = Config.Bind("LevelSpoofing",
                                "SpoofingValue",
                                "",
                                "Spoofed Levels are temporary levels that stay until the mod is inactive. Levels with it's value within 0 and 4294967295 are allowed. Decimal values won't work."); 
     
        Harmony.PatchAll();
    }
}

namespace LevelSpoofing
{
    [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.Update))]
    public static class EOSManager_Update
    {
        public static void Prefix()
        {
            ModManager.Instance.ShowModStamp();
        }
        public static uint levelvalue;
        public static void Postfix()
        {
            if (!string.IsNullOrEmpty(LevelSpooferPlugin.LevelSpoofer.Value) &&
                uint.TryParse(LevelSpooferPlugin.LevelSpoofer.Value, out levelvalue) &&
                levelvalue != DataManager.Player.Stats.Level)
            {
                DataManager.Player.stats.level = levelvalue - 1;
                DataManager.Player.Save();
            }
        }
    }
}