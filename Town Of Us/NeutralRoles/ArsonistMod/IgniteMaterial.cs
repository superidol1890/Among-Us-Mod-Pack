using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.NeutralRoles.ArsonistMod
{
    public class IgniteMaterial
    {
        public Transform transform;
    }

    [HarmonyPatch]
    public static class IgniteExtentions
    {
        public static void ClearIgnite(this IgniteMaterial b)
        {
            Object.Destroy(b.transform.gameObject);
            b = null;
        }

        public static IgniteMaterial CreateIgnite(this Vector3 location)
        {
            var IgnitePref = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            IgnitePref.name = "Ignite";
            IgnitePref.transform.localScale = new Vector3(CustomGameOptions.IgniteRadius * ShipStatus.Instance.MaxLightRadius * 2f,
                CustomGameOptions.IgniteRadius * ShipStatus.Instance.MaxLightRadius * 2f, CustomGameOptions.IgniteRadius * ShipStatus.Instance.MaxLightRadius * 2f);
            GameObject.Destroy(IgnitePref.GetComponent<SphereCollider>());
            IgnitePref.GetComponent<MeshRenderer>().material = Roles.Arsonist.igniteMaterial;
            IgnitePref.transform.position = location;
            var IgniteScript = new IgniteMaterial();
            IgniteScript.transform = IgnitePref.transform;
            return IgniteScript;
        }
    }
}