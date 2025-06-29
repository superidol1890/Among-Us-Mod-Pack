using MiraAPI.Modifiers;

namespace LaunchpadReloaded.Modifiers;

public class PoisonModifier : BaseModifier
{
    public override string ModifierName => "Poisoned";
    public override bool HideOnUi => true;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
