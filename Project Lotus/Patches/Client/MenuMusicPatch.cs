using HarmonyLib;
using Lotus.GUI;
using UnityEngine;

namespace Lotus.Patches.Client;

[HarmonyPatch(typeof(SoundStarter), nameof(SoundStarter.Awake))]
public class MenuMusicPatch
{
    public static void Prefix(SoundStarter __instance)
    {
        if (__instance.Name == "MainBG") __instance.SoundToPlay = LotusAssets.LoadAsset<AudioClip>("amongusmenu.mp3");
    }
}