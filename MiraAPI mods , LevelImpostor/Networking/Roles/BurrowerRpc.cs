using LaunchpadReloaded.Roles.Impostor;
using Reactor.Networking.Attributes;
using System.Linq;
using UnityEngine;

namespace LaunchpadReloaded.Networking.Roles;
public static class BurrowerRpc
{
    [MethodRpc((uint)LaunchpadRpc.DigVent)]
    public static void RpcDigVent(this PlayerControl playerControl)
    {
        if (playerControl.Data.Role is not BurrowerRole burrower)
        {
            playerControl.KickForCheating();
            return;
        }

        var prefab = ShipStatus.Instance.AllVents.FirstOrDefault();

        if (prefab is null)
        {
            return;
        }

        var newVent = Object.Instantiate(prefab, ShipStatus.Instance.transform);
        var ventId = Enumerable.Range(0, int.MaxValue)
                     .First(id => ShipStatus.Instance.AllVents.All(v => v.Id != id));

        newVent.transform.position = playerControl.transform.position + new Vector3(0, 0, 0.1f);
        newVent.Id = ventId;

        newVent.Left = null;
        newVent.Center = null;
        newVent.Right = null;

        if (burrower.DugVents.Count > 0)
        {
            var otherVent = burrower.DugVents.Last();
            newVent.Left = otherVent;
            otherVent.Right = newVent;
        }

        var allVents = ShipStatus.Instance.AllVents.ToList();
        allVents.Add(newVent);
        ShipStatus.Instance.AllVents = allVents.ToArray();

        burrower.DugVents.Add(newVent);
    }
}