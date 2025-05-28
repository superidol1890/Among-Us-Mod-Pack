using HarmonyLib;
using Lotus.Options;
using Lotus.API.Odyssey;

namespace Lotus.Patches.Network;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
class ShipFixedUpdatePatch
{
    public static void Postfix(ShipStatus __instance)
    {
        //ここより上、全員が実行する
        if (!AmongUsClient.Instance.AmHost) return;

        Game.CurrentGameMode.FixedUpdate();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
class StartPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(StartPatch));

    public static void Postfix()
    {
        log.Info("-----------Start Game-----------", "Phase");
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
class CheckTaskCompletionPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (!GeneralOptions.DebugOptions.NoGameEnd) return true;

        __result = false;

        return false;
    }
}