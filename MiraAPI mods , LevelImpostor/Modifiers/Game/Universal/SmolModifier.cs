using LaunchpadReloaded.Modifiers.Game;
using LaunchpadReloaded.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadReloaded.Modifiers.Fun;

public sealed class SmolModifier : LPModifier
{
    public override string ModifierName => "Smol";
    public override string Description => "You are smaller than\nthe average player.";
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SmolChance;
    public override int GetAmountPerGame() => 1;
    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && !role.Player.HasModifier<GiantModifier>();

    public override void OnActivate()
    {
        Player.MyPhysics.Speed /= 0.75f;
        Player.transform.localScale *= 0.7f;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed *= 0.75f;
        Player.transform.localScale /= 0.7f;
    }
}