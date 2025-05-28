using System;
using System.Collections.Generic;
using Lotus.Roles.Internals.Enums;
using Lotus.Extensions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Crew;
using Lotus.Roles.RoleGroups.Impostors;
using Lotus.Roles.RoleGroups.Neutral;
using Lotus.Roles.Subroles;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities.Optionals;
using Lotus.Roles.Internals;
using Lotus.Chat;
using VentLib.Utilities.Extensions;
using Lotus.API.Vanilla.Meetings;
using UnityEngine;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Holders;
using System.Linq;
using Lotus.API;
using VentLib.Utilities;

namespace Lotus.Roles.Subroles;

public class LastResort : Subrole
{
    /// <summary>
    /// A list of roles that Last Resort is not compatible with. Add your role to this list to make it not be assigned with your role.
    /// </summary>
    public static readonly List<Type> IncompatibleRoles = new()
    {
        typeof(Mafioso),
        typeof(Swapper),
        typeof(Mayor),
        typeof(Vigilante),
        typeof(Pirate),
        typeof(Assassin),
        typeof(Genie),
        typeof(Bodyguard),
        typeof(Medic),
        typeof(Oracle),
        typeof(Guesser),
        typeof(Turncoat)
    };
    public override string Identifier() => "»";

    // options
    private bool restrictedToCompatibleRoles;

    // variables
    private int additionalVotes;
    private bool hasRevealed = false;

    [RoleAction(LotusActionType.Vote, priority: Priority.High)]
    private void OnLocalPlayerVote(Optional<PlayerControl> voted, MeetingDelegate meetingDelegate, ActionHandle handle)
    {
        if (hasRevealed)
        {
            for (int i = 0; i < additionalVotes; i++) meetingDelegate.CastVote(MyPlayer, voted);
            return;
        }
        if (!voted.Exists()) return;
        if (voted.Get().PlayerId != MyPlayer.PlayerId) return;
        handle.Cancel();
        hasRevealed = true;
        ChatHandler.Of(Translations.RevealText.Formatted(MyPlayer.name, RoleName + "/" + MyPlayer.PrimaryRole().RoleName), RoleColor.Colorize(RoleName)).LeftAlign().Send();
        MyPlayer.NameModel().GetComponentHolder<RoleHolder>().Components().ForEach(component => component.SetViewerSupplier(() => PlayerControl.AllPlayerControls.ToArray().ToList()));
    }

    [RoleAction(LotusActionType.RoundEnd,  priority: API.Priority.Last)]
    private void OnReportBody() => ChatHandler.Of(Translations.HintText, RoleName).LeftAlign(false).Send(MyPlayer);

    public override HashSet<Type>? RestrictedRoles()
    {
        HashSet<Type>? restrictedRoles = base.RestrictedRoles();
        if (!restrictedToCompatibleRoles) return restrictedRoles;
        IncompatibleRoles.ForEach(r => restrictedRoles?.Add(r));
        return restrictedRoles;
    }

    public override CompatabilityMode RoleCompatabilityMode => CompatabilityMode.Blacklisted;

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.2f, 0.4f, 0.76f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Restrict to Compatible Roles", Rogue.Translations.Options.RestrictToCompatbileRoles)
                .BindBool(b => restrictedToCompatibleRoles = b)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .KeyName("Additional Votes", Translations.Options.AdditionalVotes)
                .BindInt(i => additionalVotes = i)
                .AddIntRange(1, ModConstants.MaxPlayers, 1)
                .Build());

    [Localized(nameof(LastResort))]
    public static class Translations
    {
        [Localized(nameof(RevealText))]
        public static string RevealText = "{0} was revealed as the {1}!";

        [Localized(nameof(HintText))]
        public static string HintText = "Vote yourself to reveal your role. You can vote any other player normally however.";

        [Localized(ModConstants.Options)]
        internal static class Options
        {
            [Localized(nameof(AdditionalVotes))]
            public static string AdditionalVotes = "Additional Votes";
        }
    }
}