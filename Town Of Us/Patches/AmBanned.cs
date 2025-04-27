using AmongUs.Data.Player;
using HarmonyLib;

namespace TownOfUs
{
    [HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
    public class AmBanned
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
}