using Lotus.API;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Options.UI;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Jackal : NeutralKillingBase
{
    private bool canVent;
    private bool impostorVision;

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub
                .KeyName("Can Vent", RoleTranslations.CanVent)
                .BindBool(v => canVent = v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .KeyName("Can Sabotage", RoleTranslations.CanSabotage)
                .BindBool(v => canSabotage = v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .KeyName("Impostor Vision", RoleTranslations.ImpostorVision)
                .BindBool(v => impostorVision = v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0f, 0.71f, 0.92f))
            .CanVent(canVent)
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod(), () => !impostorVision);
}