using LaunchpadReloaded.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using System.Linq;
using MiraAPI.Modifiers;

namespace LaunchpadReloaded.Modifiers;

[MiraIgnore]
public abstract class LPModifier : GameModifier
{
    public abstract string Description { get; }
    public virtual bool RemoveOnDeath => true;
    public override int GetAmountPerGame() => 1;

    public override string GetHudString()
    {
        return base.GetHudString() + $"\n<size=75%>{Description}</size>";
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (OptionGroupSingleton<LpModifierOptions>.Instance.ModifierLimit == 0) return true;
        return role.Player.GetModifierComponent().ActiveModifiers.OfType<LPModifier>().Count() < OptionGroupSingleton<LpModifierOptions>.Instance.ModifierLimit;
    }

    public override void OnDeath(DeathReason reason)
    {
        if (!RemoveOnDeath) return;
        ModifierComponent!.RemoveModifier(this);
    }
}