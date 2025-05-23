using AmongUs.Data;
using EHR.Modules;
using EHR.Patches;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using TMPro;
using UnityEngine;
using static EHR.Translator;

namespace EHR;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
internal static class MakePublicPatch
{
    public static bool Prefix()
    {
        if (ModUpdater.IsBroken || (ModUpdater.HasUpdate && ModUpdater.ForceUpdate) || !VersionChecker.IsSupported)
        {
            var message = string.Empty;
            if (!VersionChecker.IsSupported) message = GetString("UnsupportedVersion");

            if (ModUpdater.IsBroken) message = GetString("ModBrokenMessage");

            if (ModUpdater.HasUpdate) message = GetString("CanNotJoinPublicRoomNoLatest");

            Logger.Info(message, "MakePublicPatch");
            Logger.SendInGame(message);
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
// ReSharper disable once InconsistentNaming
internal static class MMOnlineManagerStartPatch
{
    public static void Postfix()
    {
        if (!((ModUpdater.HasUpdate && ModUpdater.ForceUpdate) || ModUpdater.IsBroken)) return;

        GameObject obj = GameObject.Find("FindGameButton");

        if (obj)
        {
            obj.SetActive(false);
            TextMeshPro textObj = Object.Instantiate(obj.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>());
            textObj.transform.position = new(1f, -0.3f, 0);
            textObj.name = "CanNotJoinPublic";

            string message = ModUpdater.IsBroken
                ? $"<size=2>{Utils.ColorString(Color.red, GetString("ModBrokenMessage"))}</size>"
                : $"<size=2>{Utils.ColorString(Color.red, GetString("CanNotJoinPublicRoomNoLatest"))}</size>";

            LateTask.New(() => { textObj.text = message; }, 0.01f, "CanNotJoinPublic");
        }
    }
}

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
internal static class SplashLogoAnimatorPatch
{
    public static void Prefix(SplashManager __instance)
    {
        __instance.sceneChanger.AllowFinishLoadingScene();
        __instance.startedSceneLoad = true;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
internal static class RunLoginPatch
{
    public const int ClickCount = 0;

    public static void Prefix(ref bool canOnline)
    {
        if (DebugModeManager.AmDebugger) canOnline = true;
    }
}

[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
internal static class BanMenuSetVisiblePatch
{
    public static bool Prefix(BanMenu __instance, bool show)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        show &= PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null;
        __instance.BanButton.gameObject.SetActive(AmongUsClient.Instance.CanBan());
        __instance.KickButton.gameObject.SetActive(AmongUsClient.Instance.CanKick());
        __instance.MenuButton.gameObject.SetActive(show);
        return false;
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanBan))]
internal static class InnerNetClientCanBanPatch
{
    public static bool Prefix(InnerNetClient __instance, ref bool __result)
    {
        __result = __instance.AmHost;
        return false;
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.KickPlayer))]
internal static class KickPlayerPatch
{
    public static bool Prefix( /*InnerNetClient __instance,*/ int clientId, bool ban)
    {
        if (!AmongUsClient.Instance.AmHost && !OnGameJoinedPatch.JoiningGame) return true;

        if (AmongUsClient.Instance.ClientId == clientId)
        {
            Logger.SendInGame($"Game Attempting to {(ban ? "Ban" : "Kick")} Host, Blocked the attempt.");
            return false;
        }

        if (ban) BanManager.AddBanPlayer(AmongUsClient.Instance.GetRecentClient(clientId));

        return true;
    }
}

[HarmonyPatch(typeof(ResolutionManager), nameof(ResolutionManager.SetResolution))]
internal static class SetResolutionManager
{
    public static void Postfix()
    {
        if (MainMenuManagerPatch.UpdateButton != null) MainMenuManagerPatch.UpdateButton.transform.localPosition = MainMenuManagerPatch.Template.transform.localPosition + new Vector3(0.25f, 0.75f);
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendAllStreamedObjects))]
internal static class InnerNetObjectSerializePatch
{
    public static void Prefix()
    {
        if (AmongUsClient.Instance.AmHost) Main.Instance.StartCoroutine(GameOptionsSender.SendAllGameOptionsAsync());
    }
}

[HarmonyPatch(typeof(NetworkedPlayerInfo), nameof(NetworkedPlayerInfo.Serialize))]
internal static class NetworkedPlayerInfoSerializePatch
{
    public static int IgnorePatchTimes;

    public static bool Prefix(NetworkedPlayerInfo __instance, MessageWriter writer, bool initialState, ref bool __result)
    {
        if (IgnorePatchTimes > 0)
        {
            IgnorePatchTimes--;
            return true;
        }

        writer.Write(__instance.PlayerId);
        writer.WritePacked(__instance.ClientId);
        writer.Write((byte)__instance.Outfits.Count);

        foreach (KeyValuePair<PlayerOutfitType, NetworkedPlayerInfo.PlayerOutfit> keyValuePair in __instance.Outfits)
        {
            writer.Write((byte)keyValuePair.Key);

            if (keyValuePair.Key is PlayerOutfitType.Default)
            {
                NetworkedPlayerInfo.PlayerOutfit oldOutfit = keyValuePair.Value;
                NetworkedPlayerInfo.PlayerOutfit playerOutfit = new();
                Main.AllClientRealNames.TryGetValue(__instance.ClientId, out string name);

                if (CheckForEndVotingPatch.TempExiledPlayer != null && CheckForEndVotingPatch.TempExiledPlayer.ClientId == __instance.ClientId)
                    name = CheckForEndVotingPatch.EjectionText;

                playerOutfit.Set(name ?? " ", oldOutfit.ColorId, oldOutfit.HatId, oldOutfit.SkinId, oldOutfit.VisorId, oldOutfit.PetId, oldOutfit.NamePlateId);
                playerOutfit.Serialize(writer);
            }
            else
                keyValuePair.Value.Serialize(writer);
        }

        writer.WritePacked(__instance.PlayerLevel);
        byte b = 0;
        if (__instance.Disconnected) b |= 1;
        if (__instance.IsDead) b |= 4;
        writer.Write(b);
        writer.Write((ushort)__instance.Role.Role);
        writer.Write(false);

        if (__instance.Tasks != null)
        {
            writer.Write((byte)__instance.Tasks.Count);

            for (var i = 0; i < __instance.Tasks.Count; i++)
                __instance.Tasks[i].Serialize(writer);
        }
        else
            writer.Write(0);

        writer.Write(__instance.FriendCode ?? string.Empty);

        if (GameStates.CurrentServerType == GameStates.ServerType.Vanilla)
            writer.Write(__instance.Puid ?? string.Empty);
        else
            writer.Write(string.Empty);

        if (!initialState) __instance.ClearDirtyBits();
        __result = true;
        return false;
    }
}

// https://github.com/Rabek009/MoreGamemodes/blob/master/Patches/ClientPatch.cs

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CheckOnlinePermissions))]
static class CheckOnlinePermissionsPatch
{
    public static void Prefix()
    {
        DataManager.Player.Ban.banPoints = 0f;
    }
}

[HarmonyPatch]
internal static class AuthTimeoutPatch
{
    [HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
    [HarmonyPatch(typeof(AuthManager._CoWaitForNonce_d__6), nameof(AuthManager._CoWaitForNonce_d__6.MoveNext))]
    [HarmonyPrefix]
    // From Reactor.gg
    // https://github.com/NuclearPowered/Reactor/blob/master/Reactor/Patches/Miscellaneous/CustomServersPatch.cs
    public static bool CoWaitforNoncePrefix(ref bool __result)
    {
        if (GameStates.CurrentServerType == GameStates.ServerType.Vanilla) return true;

        __result = false;
        return false;
    }

    // If you don't patch this, you still need to wait for 5 s.
    // I have no idea why this is happening
    [HarmonyPatch(typeof(AmongUsClient._CoJoinOnlinePublicGame_d__49), nameof(AmongUsClient._CoJoinOnlinePublicGame_d__49.MoveNext))]
    [HarmonyPrefix]
    public static void EnableUdpMatchmakingPrefix(AmongUsClient._CoJoinOnlinePublicGame_d__49 __instance)
    {
        // Skip to state 1, which just calls CoJoinOnlineGameDirect
        if (__instance.__1__state == 0 && !ServerManager.Instance.IsHttp)
        {
            __instance.__1__state = 1;

            __instance.__8__1 = new()
            {
                matchmakerToken = string.Empty
            };
        }
    }
}