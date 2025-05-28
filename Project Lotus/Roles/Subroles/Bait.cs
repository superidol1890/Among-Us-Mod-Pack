using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Subroles;

public class Bait : Subrole
{
    private bool announceBait;
    private bool triggered;

    [RoleAction(LotusActionType.PlayerDeath)]
    public void BaitDies(PlayerControl killer, Optional<FrozenPlayer> realKiller)
    {
        if (triggered) return;
        if (Game.State is not GameState.Roaming) return;
        triggered = true;
        realKiller.FlatMap(rk => new UnityOptional<PlayerControl>(rk.MyPlayer)).OrElse(killer).ReportDeadBody(MyPlayer.Data);
    }

    [RoleAction(LotusActionType.RoundEnd)]
    private void AnnounceBait()
    {
        if (!announceBait) return;
        announceBait = false;
        ChatHandler.Of(Translations.BaitAnnounceMessage.Formatted(MyPlayer.name), RoleColor.Colorize(RoleName)).Send();
    }

    public override string Identifier() => "★";

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier)
        .RoleColor(new Color(0f, 0.7f, 0.7f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream))
        .SubOption(sub => sub
            .KeyName("Announce Bait", Translations.Options.AnnounceBait)
            .AddBoolean(false)
            .BindBool(b => announceBait = b)
            .Build());

    [Localized(nameof(Bait))]
    public static class Translations
    {
        [Localized(nameof(BaitAnnounceMessage))] public static string BaitAnnounceMessage = "{0} is the Bait! When they die, the killer will automatically self-report their body.";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(AnnounceBait))] public static string AnnounceBait = "Announce Bait at First Meeting";
        }
    }
}