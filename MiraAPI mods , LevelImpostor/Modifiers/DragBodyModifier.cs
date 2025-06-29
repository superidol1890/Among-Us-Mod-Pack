using LaunchpadReloaded.Options.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadReloaded.Modifiers;

public class DragBodyModifier : BaseModifier
{
    public override string ModifierName => "Drag Body";

    public override bool HideOnUi => true;

    private byte BodyId { get; }
    private DeadBody? DeadBody { get; }

    private float _prevSpeed;

    public DragBodyModifier(byte bodyId)
    {
        BodyId = bodyId;
        DeadBody = Helpers.GetBodyById(BodyId);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnActivate()
    {
        _prevSpeed = Player.MyPhysics.Speed;
        if (Player != null)
        {
            Player.MyPhysics.Speed = OptionGroupSingleton<JanitorOptions>.Instance.DragSpeed;
        }
    }

    public override void OnDeactivate()
    {
        if (!Player)
        {
            return;
        }

        Player.MyPhysics.Speed = _prevSpeed;
    }

    public override void Update()
    {
        if (BodyId == 255)
        {
            return;
        }

        if (!DeadBody || !Player)
        {
            return;
        }

        DeadBody!.transform.position = Vector3.Lerp(DeadBody.transform.position, Player.transform.position, 5f * Time.deltaTime);
    }
}