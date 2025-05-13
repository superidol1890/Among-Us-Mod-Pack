using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class BloodKnight : NeutralKillingBase
{
    private float protectionAmt;
    private bool canVent;
    private bool isProtected;

    public override bool CanSabotage() => false;

    // Usually I use Misc but because the Blood Knight's color is hard to see I'm displaying this next to the player's name which requires a bit more hacky code
    [UIComponent(UI.Counter)]
    private string ProtectedIndicator() => isProtected ? RoleColor.Colorize("•") : "";

    [RoleAction(LotusActionType.RoundStart)]
    public void Reset() => isProtected = false;

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        // Call to Impostor.TryKill()
        bool killed = base.TryKill(target);
        // Possibly died due to veteran
        if (MyPlayer.Data.IsDead) return killed;

        isProtected = true;
        Async.Schedule(() => isProtected = false, protectionAmt);
        return killed;
    }

    [RoleAction(LotusActionType.Interaction)]
    private void InteractedWith(Interaction interaction, ActionHandle handle)
    {
        if (!isProtected) return;
        if (interaction.Intent is not IFatalIntent) return;
        handle.Cancel();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
         AddKillCooldownOptions(base.RegisterOptions(optionStream), name: RoleTranslations.KillCooldown)
             .Tab(DefaultTabs.NeutralTab)
             .SubOption(opt => opt
                .KeyName("Protection Duration", Translations.Options.ProtectDuration)
                .BindFloat(v => protectionAmt = v)
                .AddFloatRange(2.5f, 180, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(opt => opt
                .KeyName("Can Vent", RoleTranslations.CanVent)
                .BindBool(v => canVent = v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        return base.Modify(roleModifier)
            .RoleName("BloodKnight")
            .RoleColor(new Color(0.47f, 0f, 0f))
            .CanVent(canVent);
    }

    [Localized(nameof(BloodKnight))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(ProtectDuration))]
            public static string ProtectDuration = "Protect Duration";
        }
    }
}