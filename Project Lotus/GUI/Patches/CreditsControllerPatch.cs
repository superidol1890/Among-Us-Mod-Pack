using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Lotus.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using Object = UnityEngine.Object;

namespace Lotus.GUI.Patches;

public static class CreditsControllerPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(CreditsControllerPatch));

    private const bool TestCredits = false;

    private static void PassCreditsController(GameObject mainObject)
    {
        mainObject.transform.parent.FindChild("FollowUs").gameObject.SetActive(false); // nobody is following my facebook!!!!!! 😡😡
        // mainObject.transform.localPosition -= new Vector3(0, 5, 0);

        // now we have to add a sprite renderer.
        var creditsBG = new GameObject("CreditsBackground");
        creditsBG.transform.SetParent(mainObject.transform.parent);
        creditsBG.transform.localPosition = mainObject.transform.localPosition;
        creditsBG.transform.localScale = Vector3.one;

        creditsBG.transform.localPosition += new Vector3(0.1f, 0, 0);

        var renderer = creditsBG.AddComponent<SpriteRenderer>();
        renderer.sprite = LotusAssets.LoadSprite("Credits/Images/background.png", 180);
    }

    [QuickPrefix(typeof(CreditsController), nameof(CreditsController.LoadCredits))]
    public static void LoadCreditsPrefix(CreditsController __instance) => DestroyableSingleton<ReferenceDataManager>.Instance.Refdata.credits =
        LotusAssets.LoadAsset<TextAsset>("Credits/" + (TestCredits ? "testcredits" : "plcredits") + ".txt");

    [QuickPrefix(typeof(CreditsController), nameof(CreditsController.Start))]
    public static bool StartPrefix(CreditsController __instance)
    {
        __instance.initialDelay = 1f;
        __instance.remainingDelay = __instance.initialDelay;
        if (TestCredits)
        {
            __instance.startPos = -800f;
            __instance.OnEnable();
        }
        log.Debug("First Credit: " + __instance.credits[0].columns[0]);
        GameObject mainObject = __instance.gameObject;
        PassCreditsController(mainObject);
        for (int i = 0; i < __instance.credits.Count; i++)
        {
            CreditsController.FormatStruct format = __instance.GetFormat(__instance.credits[i].format);
            GameObject gameObject2 = Object.Instantiate<GameObject>(__instance.creditPanelPrefab, __instance.creditMainPanel.transform);
            for (int j = 0; j < format.formatPrefabs.Count; j++)
            {
                GameObject gameObject3 = Object.Instantiate<GameObject>(format.formatPrefabs[j], gameObject2.transform);
                if (format.creditType == CreditsController.CreditType.TEXT)
                {
                    string text = __instance.credits[i].columns[j].Trim();
                    if (text.IsNullOrWhiteSpace())
                    {
                        gameObject3.GetComponent<TextMeshProUGUI>().text = string.Empty;
                    }
                    else
                    {
                        TextMeshProUGUI component = gameObject3.GetComponent<TextMeshProUGUI>();
                        component.text += text;
                    }
                }
                else if (format.creditType == CreditsController.CreditType.IMAGE)
                {
                    string creditsPath = __instance.credits[i].columns[j];
                    if (!creditsPath.Contains("Credits/Images/")) gameObject3.GetComponent<Image>().sprite = Resources.Load<Sprite>("Credits/" + __instance.credits[i].columns[j]);
                    else
                    {
                        string[] assetSplit = creditsPath.Split(";;");
                        gameObject3.GetComponent<Image>().sprite = LotusAssets.LoadSprite(assetSplit[1], float.Parse(assetSplit[0]));
                        if (assetSplit.Length > 2)
                        {
                            float[] sizeValues = assetSplit[2].Split(";").Select(float.Parse).ToArray();
                            gameObject3.transform.localScale = new Vector3(sizeValues[0], sizeValues[1], sizeValues[2]);
                        }
                    }

                    gameObject3.GetComponent<Image>().preserveAspect = true;
                }
            }
        }
        return false;
    }

    [QuickPostfix(typeof(CreditsController), nameof(CreditsController.OnEnable))]
    public static void CreditsOnEnable()
    {
        var playOnlineAnchor = GameObject.Find("PlayOnlineAnchor");
        if (playOnlineAnchor != null) playOnlineAnchor.SetActive(false);
    }

    [QuickPrefix(typeof(CreditsController), nameof(CreditsController.FixedUpdate))]
    public static bool FixedUpdate(CreditsController __instance)
    {
        if (!TestCredits) return true;
        if (__instance.paused) return false;
        if (__instance.remainingDelay > 0f)
        {
            __instance.remainingDelay -= Time.unscaledDeltaTime;
            return false;
        }

        float endHeight = __instance.creditsRect.rect.height * 2.5f;
        float num = Mathf.Clamp(__instance.creditMainPanel.transform.localPosition.y + __instance.creditScrollSpeed, float.MinValue, endHeight);
        __instance.creditMainPanel.transform.localPosition = new Vector2(__instance.creditMainPanel.transform.localPosition.x, num);

        if (__instance.creditsRect.localPosition.y >= endHeight && __instance.OnFinish != null) return true;
        return false;
    }
}