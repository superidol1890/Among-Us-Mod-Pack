using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
public static class ServerDropdownPatch
{
    public static bool Prefix(ServerDropdown __instance)
    {
        var num = 0;
        __instance.background.size = new Vector2(8.4f, 4.8f);

        foreach (var regionInfo in DestroyableSingleton<ServerManager>.Instance.AvailableRegions)
        {
            if (DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Equals(regionInfo))
            {
                __instance.defaultButtonSelected = __instance.firstOption;
                __instance.firstOption.ChangeButtonText(
                    DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(
                        regionInfo.TranslateName,
                        regionInfo.Name));
            }
            else
            {
                var region = regionInfo;
                var serverListButton = __instance.ButtonPool.Get<ServerListButton>();
                var x = num % 2 == 0 ? -2 : 2;
                var y = -0.55f * (num / 2);
                serverListButton.transform.localPosition = new Vector3(x, __instance.y_posButton + y, -1f);
                serverListButton.transform.localScale = Vector3.one;
                serverListButton.Text.text =
                    DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(
                        regionInfo.TranslateName,
                        regionInfo.Name);
                serverListButton.Text.ForceMeshUpdate();
                serverListButton.Button.OnClick.RemoveAllListeners();
                serverListButton.Button.OnClick.AddListener((UnityAction)(() => { __instance.ChooseOption(region); }));
                __instance.controllerSelectable.Add(serverListButton.Button);
                __instance.background.transform.localPosition = new Vector3(
                    0f,
                    __instance.initialYPos + (-0.3f * (num / 2)),
                    0f);
                __instance.background.size = new Vector2(__instance.background.size.x, 1.2f + (0.6f * (num / 2)));
                num++;
            }
        }

        return false;
    }
}
