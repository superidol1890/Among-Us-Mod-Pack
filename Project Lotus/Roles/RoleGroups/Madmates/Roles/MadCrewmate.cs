using AmongUs.GameOptions;
using Lotus.API;
using Lotus.Factions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Options;
using UnityEngine;
using VentLib.Options.UI;

namespace Lotus.Roles.RoleGroups.Madmates.Roles;

public abstract class MadCrewmate : Engineer
{
    private bool canVent;
    private bool impostorVision;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddTaskOverrideOptions(base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Has Impostor Vision")
                .AddBoolean()
                .BindBool(b => impostorVision = b)
                .Build())
            .SubOption(sub => AddVentingOptions(sub)
                .KeyName("Can Vent", RoleTranslations.CanVent)
                .AddBoolean()
                .BindBool(b => canVent = b)
                .Build()));

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(canVent ? RoleTypes.Engineer : RoleTypes.Crewmate)
            .SpecialType(SpecialType.Madmate)
            .RoleColor(new Color(0.73f, 0.18f, 0.02f))
            .Faction(FactionInstances.Madmates)
            .OptionOverride(Override.CrewLightMod, () => AUSettings.CrewLightMod(), () => impostorVision);
}
