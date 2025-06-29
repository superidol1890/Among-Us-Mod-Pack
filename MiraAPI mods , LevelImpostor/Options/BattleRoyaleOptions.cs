using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace LaunchpadReloaded.Options;

public class BattleRoyaleOptions : AbstractOptionGroup
{
    public override string GroupName => "Battle Royale Options";

    public override Func<bool> GroupVisible => () => false; //CustomGameModeManager.ActiveMode?.GetType() == typeof(BattleRoyale);

    [ModdedToggleOption("Use Seeker Character")] public bool SeekerCharacter { get; set; } = true;
    
    public ModdedToggleOption ShowKnife { get; } = new("Show Knife", true)
    {
        Visible = () => OptionGroupSingleton<BattleRoyaleOptions>.Instance.SeekerCharacter
    };
}