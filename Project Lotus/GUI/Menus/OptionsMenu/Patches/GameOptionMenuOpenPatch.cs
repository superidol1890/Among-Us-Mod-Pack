using HarmonyLib;
using VentLib.Utilities;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.GUI.Menus.OptionsMenu.Patches;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public class GameOptionMenuOpenPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(GameOptionMenuOpenPatch));

    public static CustomOptionContainer OptionContainer = null!;
    public static OptionsMenuBehaviour MenuBehaviour = null!;
    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        MenuBehaviour = __instance;
        __instance.transform.localPosition -= new UnityEngine.Vector3(0.02f, 0f, 0f);
        OptionContainer = __instance.gameObject.AddComponent<CustomOptionContainer>();
        OptionContainer.PassMenu(__instance);
        Async.Schedule(() => __instance.transform.localScale = new UnityEngine.Vector3(0.9714f, 0.9714f, 1f), __instance.gameObject.GetComponent<TransitionOpen>().duration + .2f);
        Async.Schedule(() => __instance.BackButton.transform.localPosition = new UnityEngine.Vector3(-4.73f, 2.8f, -1), __instance.gameObject.GetComponent<TransitionOpen>().duration + .2f);
    }

    [QuickPrefix(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.ResetText))]
    public static bool DisableResetTextFunc(OptionsMenuBehaviour __instance) => false;
}