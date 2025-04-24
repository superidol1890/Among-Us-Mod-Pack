using LaunchpadReloaded.Components;
using LaunchpadReloaded.Roles.Impostor;
using LaunchpadReloaded.Utilities;
using Reactor.Networking.Attributes;
using System.Linq;
using UnityEngine;
using Helpers = MiraAPI.Utilities.Helpers;

namespace LaunchpadReloaded.Networking;

public static class DeadBodyRpc
{
    [MethodRpc((uint)LaunchpadRpc.HideBodyInVent)]
    public static void RpcHideBodyInVent(this PlayerControl pc, byte bodyId, int ventId)
    {
        if (pc.Data.Role is not JanitorRole)
        {
            pc.KickForCheating();
            return;
        }

        var body = Helpers.GetBodyById(bodyId);
        var vent = ShipStatus.Instance.AllVents.First(v => v.Id == ventId);

        if (body == null || vent == null)
        {
            return;
        }

        if (!vent.TryGetComponent<VentBodyComponent>(out var ventBody))
        {
            ventBody = vent.gameObject.AddComponent<VentBodyComponent>();
        }

        body.HideBody();
        var transform = body.transform;
        var pos = transform.position;
        var pos2 = vent.transform.position;
        transform.position = new Vector3(pos2.x, pos2.y, pos.z);
        ventBody.deadBody = body;
    }
}