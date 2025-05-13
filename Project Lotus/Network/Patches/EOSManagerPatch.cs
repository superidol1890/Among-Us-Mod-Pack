using System.Collections;
using AmongUs.Data;
using HarmonyLib;
using Lotus.Logging;
using Lotus.Network.PrivacyPolicy;
using UnityEngine.Networking;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
// using Lotus.Network.Komasan.RestClient;

namespace Lotus.Network.Patches;

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
public class EOSManagerPatch
{
    private static bool hasInitialized = false;
    public static void Postfix(EOSManager __instance, bool canOnline)
    {
        if (!canOnline || hasInitialized) return;
        hasInitialized = true;
        Async.Execute(CreateForum(__instance));
        // return;
        // Komajiro komajiro = Komajiro.Instance;
        // if (!canOnline) return;
        // komajiro.Initialize(__instance.ProductUserId, __instance.platformAuthToken);
    }

    private static IEnumerator CreateForum(EOSManager __instance)
    {
        if (!ProjectLotus.DevVersion) yield break;
        if (PrivacyPolicyInfo.Instance != null)
        {
            if (!PrivacyPolicyInfo.Instance.ConnectWithAPI) yield break;
        }
        // Generate FC
        if (__instance.IsFriendsListAllowed() && (__instance.FriendCode == null || __instance.FriendCode == string.Empty))
        {
            yield return __instance.StartCoroutine(DestroyableSingleton<FriendsListManager>.Instance.CheckFriendCodeOnLogin());
        }
        string jsonData = $@"{{
            ""hostName"": ""{DataManager.Player.Customization.Name}"",
            ""friendCode"": ""{__instance.FriendCode}""
        }}";
        byte[] postData = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // Send POST request
        UnityWebRequest webRequest = new(NetConstants.Host + "createforum", UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(postData),
            downloadHandler = new DownloadHandlerBuffer()
        };
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.Success:
                break;
            default:
                DevLogger.Log("Result: {0} - Error: {1} - ResponseCode: {2} - Server Response: {3}".Formatted(webRequest.result.ToString(),
                    webRequest.error, webRequest.responseCode, webRequest.downloadHandler.text));
                break;
        }
    }
}