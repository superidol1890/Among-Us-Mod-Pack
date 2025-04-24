using LaunchpadReloaded.Features;
using LaunchpadReloaded.Roles.Impostor;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Collections;
using UnityEngine;
using Helpers = MiraAPI.Utilities.Helpers;

namespace LaunchpadReloaded.Networking.Roles;
public static class SurgeonRpc
{
    public static IEnumerator FadeOutBody(DeadBody body, PlayerControl? plr)
    {
        var rend = body.gameObject.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
        yield return Utilities.Helpers.FadeOut(rend);
        rend.transform.parent.gameObject.SetActive(false);

        if (plr != null)
        {
            plr.cosmetics.CurrentPet.gameObject.SetActive(false);
        }
    }


    [MethodRpc((uint)LaunchpadRpc.Dissect)]
    public static void RpcDissect(this PlayerControl playerControl, byte bodyId)
    {
        if (playerControl.Data.Role is not SurgeonRole)
        {
            playerControl.KickForCheating();
            return;
        }

        var body = Helpers.GetBodyById(bodyId);
        var player = GameData.Instance.GetPlayerById(bodyId);
        if (body != null)
        {
            var bone = new GameObject("Bone").AddComponent<SpriteRenderer>();
            bone.sprite = LaunchpadAssets.Bone.LoadAsset();
            bone.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            bone.transform.localPosition = new Vector3(body.transform.localPosition.x - 0.17f, body.transform.localPosition.y - 0.17f, body.transform.localPosition.z);
            bone.gameObject.layer = body.gameObject.layer;
            body.Reported = true;
            body.enabled = false;
            Coroutines.Start(FadeOutBody(body, player.Object));
        }
        else
        {
            Logger<LaunchpadReloadedPlugin>.Warning($"Body for id {bodyId} not found");
        }
    }
}