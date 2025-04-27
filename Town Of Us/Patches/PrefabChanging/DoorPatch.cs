using HarmonyLib;

namespace TownOfUs
{
    [HarmonyPatch]

    public class AirshipDoors
    {
        [HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.OnEnable))]
        [HarmonyPostfix]

        public static void Postfix(AirshipStatus __instance)
        {
            if (!CustomGameOptions.AirshipPolusDoors) return;

            var polusdoor = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
            foreach (var door in __instance.GetComponentsInChildren<DoorConsole>())
            {
                door.MinigamePrefab = polusdoor;
            }
        }
    }
}