using LaunchpadReloaded.Features;
using LaunchpadReloaded.Roles.Crewmate;
using LaunchpadReloaded.Utilities;
using Reactor.Networking.Attributes;
using UnityEngine;
using Helpers = MiraAPI.Utilities.Helpers;

namespace LaunchpadReloaded.Networking.Roles;
public static class CoronerRpc
{
    [MethodRpc((uint)LaunchpadRpc.FreezeBody)]
    public static void RpcFreezeBody(this PlayerControl playerControl, byte deadBody)
    {
        if (playerControl.Data.Role is not CoronerRole coroner)
        {
            playerControl.KickForCheating();
            return;
        }

        var body = Helpers.GetBodyById(deadBody);
        if (body != null)
        {
            body.GetCacheComponent().isFrozen = true;

            var bodyOverlay = new GameObject("FreezeOverlay").AddComponent<SpriteRenderer>();
            bodyOverlay.sprite = LaunchpadAssets.FrozenBodyOverlay.LoadAsset();
            bodyOverlay.transform.SetParent(body.transform);
            bodyOverlay.color = new UnityEngine.Color(1, 1, 1, 0.7f);
            bodyOverlay.transform.localPosition = new Vector3(-0.25f, -0.12f, -0.000001f);
            bodyOverlay.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            bodyOverlay.gameObject.layer = body.gameObject.layer;
        }
    }
}