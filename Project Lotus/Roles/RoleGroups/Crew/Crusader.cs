using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Crew.Crusader.CrusaderTranslations.CrusaderOptions;
using Lotus.Roles.Internals.Enums;
using static Lotus.Utilities.TranslationUtil;
using Lotus.Patches.Systems;
using Lotus.API.Vanilla.Sabotages;

namespace Lotus.Roles.RoleGroups.Crew;

public class Crusader : Crewmate
{
    private Optional<byte> protectedPlayer = Optional<byte>.Null();
    private bool protectAgainstHelpfulInteraction;
    private bool protectAgainstNeutralInteraction;

    [RoleAction(LotusActionType.Attack)]
    private void SelectTarget(PlayerControl target)
    {
        if (MyPlayer.InteractWith(target, LotusInteraction.HelpfulInteraction.Create(this)) == InteractionResult.Halt) return;
        protectedPlayer = Optional<byte>.NonNull(target.PlayerId);
        MyPlayer.RpcMark(target);
        Game.MatchData.GameHistory.AddEvent(new ProtectEvent(MyPlayer, target));
    }

    [RoleAction(LotusActionType.Interaction, ActionFlag.GlobalDetector)]
    private void AnyPlayerTargeted(PlayerControl target, PlayerControl killer, Interaction interaction, ActionHandle handle)
    {
        if (Game.State is not GameState.Roaming) return;
        if (killer.PlayerId == MyPlayer.PlayerId) return;
        if (!protectedPlayer.Exists()) return;
        if (target.PlayerId != protectedPlayer.Get()) return;
        Intent intent = interaction.Intent;

        switch (intent)
        {
            case IHelpfulIntent when !protectAgainstHelpfulInteraction:
            case INeutralIntent when !protectAgainstNeutralInteraction:
            case IFatalIntent fatalIntent when fatalIntent.IsRanged():
                return;
        }

        if (interaction is IDelayedInteraction or IRangedInteraction or IIndirectInteraction) return;

        handle.Cancel();
        RoleUtils.SwapPositions(target, MyPlayer);
        bool killed = MyPlayer.InteractWith(killer, LotusInteraction.FatalInteraction.Create(this)) is InteractionResult.Proceed;
        Game.MatchData.GameHistory.AddEvent(new PlayerSavedEvent(target, MyPlayer, killer));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, killer, killed));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Protect against Beneficial Interactions", Colorize(BeneficialInteractionProtection, ModConstants.Palette.PassiveColor))
                .BindBool(b => protectAgainstHelpfulInteraction = b)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub.KeyName("Protect against Neutral Interactions", Colorize(NeutralInteractionProtection, ModConstants.Palette.NeutralColor))
                .BindBool(b => protectAgainstNeutralInteraction = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .DesyncRole(RoleTypes.Impostor)
            .RoleFlags(RoleFlag.CannotWinAlone)
            .RoleColor(new Color(0.78f, 0.36f, 0.22f))
            .RoleAbilityFlags(RoleAbilityFlag.CannotVent | RoleAbilityFlag.CannotSabotage | RoleAbilityFlag.IsAbleToKill)
            .OptionOverride(new IndirectKillCooldown(() => AUSettings.KillCooldown()))
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod())
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod() / 5, () => SabotagePatch.CurrentSabotage != null && SabotagePatch.CurrentSabotage.SabotageType() is SabotageType.Lights);

    [Localized(nameof(Crusader))]
    public static class CrusaderTranslations
    {
        [Localized(ModConstants.Options)]
        public static class CrusaderOptions
        {
            [Localized(nameof(BeneficialInteractionProtection))]
            public static string BeneficialInteractionProtection = "Protect against Beneficial::0 Interactions";

            [Localized(nameof(NeutralInteractionProtection))]
            public static string NeutralInteractionProtection = "Protect against Neutral::0 Interactions";
        }
    }
}