using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.GUI.Name;
using Lotus.Roles.Internals.Enums;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Statuses;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Subroles;

public class Bewilder : Subrole
{
    private float visionMultiplier;
    public override string Identifier() => "<size=2.3>⁂</size>";

    [RoleAction(LotusActionType.PlayerDeath)]
    private void BewilderDies(PlayerControl killer, Optional<FrozenPlayer> realKiller)
    {
        if (realKiller.Exists()) killer = realKiller.Get().MyPlayer;

        GameOptionOverride optionOverride = killer.GetVanillaRole().IsImpostor()
            ? new MultiplicativeOverride(Override.ImpostorLightMod, visionMultiplier)
            : new MultiplicativeOverride(Override.CrewLightMod, visionMultiplier);


        Game.MatchData.Roles.AddOverride(killer.PlayerId, optionOverride);
        killer.PrimaryRole().SyncOptions();
        string name = killer.name;
        killer.NameModel().GCH<IndicatorHolder>().Add(new SimpleIndicatorComponent(Identifier(), RoleColor, GameState.Roaming, killer));

        CustomStatus status = CustomStatus.Of(Translations.BewilderedStatus).Description(Translations.BewilderedDescription).Color(RoleColor).StatusFlags(StatusFlag.Hidden).Build();
        Game.MatchData.AddStatus(killer, status, MyPlayer);
    }


    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.42f, 0.28f, 0.2f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Vision Multiplier", Translations.Options.VisionMultiplier)
                .AddFloatRange(0.1f, 1f, 0.05f, 9, "x")
                .BindFloat(f => visionMultiplier = f)
                .Build());


    [Localized(nameof(Bewilder))]
    public static class Translations
    {
        [Localized(nameof(BewilderedDescription))] public static string BewilderedDescription = "The Bewildered status reduces your vision by a specific multiplier.";
        [Localized(nameof(BewilderedStatus))] public static string BewilderedStatus = "Bewildered";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(VisionMultiplier))] public static string VisionMultiplier = "Vision Multiplier";
        }
    }
}