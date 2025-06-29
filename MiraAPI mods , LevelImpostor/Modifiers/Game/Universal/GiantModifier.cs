using LaunchpadReloaded.Modifiers.Fun;
using LaunchpadReloaded.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadReloaded.Modifiers.Game;

public sealed class GiantModifier : LPModifier
{
    public override string ModifierName => "Giant";
    public override string Description => "You are larger than\nthe average player.";
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantChance;
    public override int GetAmountPerGame() => 1;
    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && !role.Player.HasModifier<SmolModifier>();
    public override void OnActivate()
    {
        Player.MyPhysics.Speed *= 0.8f;
        Player.transform.localScale /= 0.7f;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed /= 0.8f;
        Player.transform.localScale *= 0.7f;
    }
}