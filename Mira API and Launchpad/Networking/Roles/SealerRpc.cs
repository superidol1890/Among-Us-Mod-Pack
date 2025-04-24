using LaunchpadReloaded.Components;
using LaunchpadReloaded.Features;
using LaunchpadReloaded.Options.Roles.Crewmate;
using LaunchpadReloaded.Roles.Crewmate;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using System.Linq;
using UnityEngine;

namespace LaunchpadReloaded.Networking.Roles;
public static class SealerRpc
{
    [MethodRpc((uint)LaunchpadRpc.SealVent)]
    public static void RpcSealVent(this PlayerControl playerControl, int ventId)
    {
        if (playerControl.Data.Role is not SealerRole sealer)
        {
            playerControl.KickForCheating();
            return;
        }
        var vent = ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == ventId);

        if (vent is null)
        {
            return;
        }

        if (OptionGroupSingleton<SealerOptions>.Instance.SealReveal && vent.gameObject.TryGetComponent<VentBodyComponent>(out var body))
        {
            body.ExposeBody();
        }

        var seal = vent.gameObject.AddComponent<SealedVentComponent>();
        seal.Sealer = playerControl;

        sealer.SealedVents.Add(seal);

        var ventTape = new GameObject("VentTape");
        ventTape.transform.SetParent(vent.transform);
        ventTape.transform.localPosition = new Vector3(0, -0.05f, -0.05f);
        ventTape.transform.localScale = new Vector3(0.55f, 0.35f, 0.4f);
        ventTape.layer = vent.gameObject.layer;

        var rend = ventTape.AddComponent<SpriteRenderer>();
        rend.color = new UnityEngine.Color(1, 1, 1, 0.5f);

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Fungle:
                rend.sprite = LaunchpadAssets.VentTapePolus.LoadAsset();
                ventTape.transform.localPosition = new Vector3(0.05f, -0.13f, -0.05f);
                ventTape.transform.localScale = new Vector3(2, 1.75f, 1f);
                rend.color = new UnityEngine.Color(1, 1, 1, 0.65f);
                break;
            case ShipStatus.MapType.Pb:
                rend.sprite = LaunchpadAssets.VentTapePolus.LoadAsset();
                ventTape.transform.localPosition = new Vector3(0.01f, 0, -0.05f);
                ventTape.transform.localScale = new Vector3(1.5f, 1.3f, 1f);
                rend.color = new UnityEngine.Color(1, 1, 1, 0.65f);
                break;
            default:
                rend.sprite = LaunchpadAssets.VentTape.LoadAsset();
                break;
        }
        //TODO; IMPLEMENT DIFFERENT TEXTURES FOR MAPS.
    }
}