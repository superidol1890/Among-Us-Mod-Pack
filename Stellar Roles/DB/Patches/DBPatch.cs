using HarmonyLib;
using UnityEngine;

namespace LevelImposter.DB;

/*
 *      Loads all ship prefabs
 *      to memory so that au assets
 *      can be stored in a db
 */
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
public static class DBPatch
{
    public static void Postfix()
    {
        var dbObj = new GameObject("AssetDB");
        Object.DontDestroyOnLoad(dbObj);
        dbObj.AddComponent<AssetDB>();
    }
}