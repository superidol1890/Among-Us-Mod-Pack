using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Options;
using UnityEngine;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Escapist : Impostor
{
    private bool clearMarkAfterMeeting;

    private Vector2? location;

    [UIComponent(UI.Cooldown)]
    private Cooldown canEscapeCooldown;

    [UIComponent(UI.Cooldown)]
    private Cooldown canMarkCooldown;

    [UIComponent(UI.Text)]
    private string TpIndicator() => canEscapeCooldown.IsReady() && location != null ? Color.red.Colorize("Press Pet to Escape") : "";

    protected override void PostSetup()
    {
        CooldownHolder cooldownHolder = MyPlayer.NameModel().GetComponentHolder<CooldownHolder>();
        cooldownHolder.Get(1).SetPrefix("Escape In: ").SetTextColor(Color.red);
    }

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(LotusActionType.OnPet)]
    private void PetAction()
    {
        if (location == null) TryMarkLocation();
        else TryEscape();
    }

    [RoleAction(LotusActionType.RoundStart)]
    private void ClearMark()
    {
        if (clearMarkAfterMeeting) location = null;
    }

    private void TryMarkLocation()
    {
        if (canMarkCooldown.NotReady()) return;
        location = MyPlayer.GetTruePosition();
        canEscapeCooldown.Start();
    }

    private void TryEscape()
    {
        if (canEscapeCooldown.NotReady() || location == null) return;
        Utils.Teleport(MyPlayer.NetTransform, location.Value);
        location = null;
        canMarkCooldown.Start();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Cooldown After Mark", Translations.Options.MarkCooldown)
                .AddFloatRange(0, 60, 2.5f, 2, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(canEscapeCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub
                .KeyName("Cooldown After Escape", Translations.Options.EscapeCooldown)
                .AddFloatRange(0, 180, 2.5f, 16, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(canMarkCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub
                .KeyName("Clear Mark After Meeting", Translations.Options.ClearAfterMeeting)
                .AddOnOffValues()
                .BindBool(b => clearMarkAfterMeeting = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleAbilityFlags(RoleAbilityFlag.UsesPet);

    [Localized(nameof(Escapist))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(MarkCooldown))]
            public static string MarkCooldown = "Cooldown After Mark";

            [Localized(nameof(EscapeCooldown))]
            public static string EscapeCooldown = "Cooldown After Escape";

            [Localized(nameof(ClearAfterMeeting))]
            public static string ClearAfterMeeting = "Clear Mark After Meeting";
        }
    }
}