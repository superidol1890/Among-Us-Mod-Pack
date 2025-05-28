using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles.Events;
using Lotus.Roles.GUI;
using Lotus.Roles.GUI.Interfaces;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.RPC;
using Lotus.Utilities;
using UnityEngine;
using VentLib;
using VentLib.Localization.Attributes;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using static Lotus.Roles.RoleGroups.Impostors.Mastermind.MastermindTranslations;
using static Lotus.Roles.RoleGroups.Impostors.Mastermind.MastermindTranslations.MastermindOptionTranslations;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Mastermind : Impostor, IRoleUI
{
    private int manipulatedPlayerLimit;
    private bool impostorsCanSeeManipulated;
    private Cooldown timeToKill = null!;

    [NewOnSetup] private HashSet<byte> manipulatedPlayers = null!;
    [NewOnSetup] private Dictionary<byte, Remote<TextComponent>?[]> remotes = null!;
    [NewOnSetup] private Dictionary<byte, Cooldown> expirationTimers = null!;
    [NewOnSetup] private Dictionary<byte, HashSet<byte>> manipuletedKills = null!;

    private bool CanManipulate => manipulatedPlayerLimit == -1 || manipulatedPlayers.Count < manipulatedPlayerLimit;

    public RoleButton KillButton(IRoleButtonEditor killButton) => UpdateKillButton(killButton);

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!CanManipulate) return base.TryKill(target);

        PlayerControl[] viewers = impostorsCanSeeManipulated
            ? Players.GetAllPlayers().Where(p => !p.IsAlive() || Relationship(p) is Relation.FullAllies).AddItem(MyPlayer).ToArray()
            : [MyPlayer];
        TextComponent alliedText = new(new LiveString(ManipulatedText, RoleColor), GameState.Roaming, ViewMode.Additive, viewers);

        ClearManipulated(target);
        Remote<TextComponent>?[] textComponents = { target.NameModel().GCH<TextHolder>().Add(alliedText), null };

        remotes[target.PlayerId] = textComponents;
        manipulatedPlayers.Add(target.PlayerId);

        Async.Schedule(() => BeginSuicideCountdown(target), 5f);
        RefreshKillCooldown(target);
        if (MyPlayer.AmOwner) UpdateKillButton(UIManager.KillButton);
        else if (MyPlayer.IsModded()) Vents.FindRPC((uint)ModCalls.UpdateMastermind)?.Send([MyPlayer.OwnerId], CanManipulate);
        return false;
    }

    [RoleAction(LotusActionType.RoundStart)]
    private void OnRoundStart() => manipuletedKills.Clear();

    [RoleAction(LotusActionType.ReportBody, ActionFlag.GlobalDetector | ActionFlag.WorksAfterDeath, priority: API.Priority.First)]
    private void StopManipulatedReport(PlayerControl reporter, Optional<NetworkedPlayerInfo> body, ActionHandle handle)
    {
        if (!body.Exists()) return; // trying to report meeting
        if (!manipuletedKills.TryGetValue(reporter.PlayerId, out HashSet<byte>? killedPlayers)) return; // they havent killed someone
        if (!killedPlayers.Contains(body.Get().PlayerId)) return; // they are reporting someone else
        handle.Cancel();
    }

    [RoleAction(LotusActionType.OnPet, ActionFlag.GlobalDetector | ActionFlag.WorksAfterDeath)]
    public void InterceptPetAction(PlayerControl petter, ActionHandle handle)
    {
        if (!manipulatedPlayers.Contains(petter.PlayerId)) return;

        PlayerControl? target = petter.GetPlayersInAbilityRangeSorted().FirstOrDefault();
        if (target == null) return;
        handle.Cancel();
        DoManipulationKill(petter, target);
    }

    [RoleAction(LotusActionType.PlayerAction, ActionFlag.GlobalDetector | ActionFlag.WorksAfterDeath)]
    public void InterceptTargetAction(PlayerControl emitter, RoleAction action, ActionHandle handle, object[] parameters)
    {
        if (!manipulatedPlayers.Contains(emitter.PlayerId)) return;
        if (action.ActionType is LotusActionType.ReportBody)
        {
            handle.Cancel();
            return;
        }
        if (action.ActionType is not LotusActionType.Attack) return;

        PlayerControl? target = (PlayerControl?)(handle.ActionType is LotusActionType.Attack ? parameters[0] : emitter.GetPlayersInAbilityRangeSorted().FirstOrDefault());
        if (target == null) return;
        handle.Cancel();
        DoManipulationKill(emitter, target);
    }

    private void DoManipulationKill(PlayerControl emitter, PlayerControl target)
    {
        CustomRole emitterRole = emitter.PrimaryRole();
        Remote<GameOptionOverride> killCooldown = Game.MatchData.Roles.AddOverride(emitter.PlayerId, new GameOptionOverride(Override.KillCooldown, 0f));
        emitterRole.SyncOptions();
        Async.Schedule(() =>
        {
            emitter.InteractWith(target, new ManipulatedInteraction(new FatalIntent(), emitter.PrimaryRole(), MyPlayer));
            killCooldown.Delete();
            emitterRole.SyncOptions();
            if (target.IsAlive()) return;
            manipuletedKills.GetOrCompute(emitter.PlayerId, () => new HashSet<byte>()).Add(target.PlayerId);
        }, NetUtils.DeriveDelay(0.05f));
        ClearManipulated(emitter);
    }

    private void BeginSuicideCountdown(PlayerControl target)
    {
        if (target == null) return;
        Cooldown playerCooldown = expirationTimers[target.PlayerId] = timeToKill.Clone();
        LiveString killIndicator = new(_ => KillImploredText.Formatted(Color.white.Colorize(playerCooldown + "s")), RoleColor);

        TextComponent textComponent = new(killIndicator, GameState.Roaming, viewers: target);
        remotes.GetOrCompute(target.PlayerId, () => [ null, null ])[1] = target.NameModel().GCH<TextHolder>().Add(textComponent);
        playerCooldown.StartThenRun(() => ExecuteSuicide(target));
    }

    private void ExecuteSuicide(PlayerControl target)
    {
        target.InteractWith(target, new UnblockedInteraction(new FatalIntent(false, () => new ManipulatedPlayerDeathEvent(target, target)), this));
    }

    [ModRPC((uint)ModCalls.UpdateMastermind, RpcActors.Host, RpcActors.NonHosts)]
    private static void RpcUpdateKillButton(bool canManipulate)
    {
        Mastermind? mastermind = PlayerControl.LocalPlayer.PrimaryRole<Mastermind>();
        if (mastermind == null) return;
        IRoleButtonEditor killButton = mastermind.UIManager.KillButton;
        if (canManipulate)
            killButton.SetText(ManipulateButtonText).SetSprite(() =>
                LotusAssets.LoadSprite("Buttons/Imp/mastermind_manipulate.png", 130, true));
        else killButton.SetText(Witch.Translations.KillButtonText).RevertSprite();
    }

    private RoleButton UpdateKillButton(IRoleButtonEditor killButton) => CanManipulate
        ? killButton.SetText(ManipulateButtonText).SetSprite(() => LotusAssets.LoadSprite("Buttons/Imp/mastermind_manipulate.png", 130, true))
        : killButton.SetText(Witch.Translations.KillButtonText).RevertSprite();

    [RoleAction(LotusActionType.PlayerDeath)] // MY DEATH
    [RoleAction(LotusActionType.RoundEnd)]
    public override void HandleDisconnect()
    {
        manipulatedPlayers.ToArray().Filter(Players.PlayerById).ForEach(p =>
        {
            FatalIntent fatalIntent = new(false, () => new ManipulatedPlayerDeathEvent(p, p));
            p.InteractWith(p, new ManipulatedInteraction(fatalIntent, p.PrimaryRole(), MyPlayer));
            ClearManipulated(p, false);
        });
        manipulatedPlayers.Clear();
    }

    [RoleAction(LotusActionType.PlayerDeath, ActionFlag.GlobalDetector | ActionFlag.WorksAfterDeath)] // EVERY OTHER PERSON'S DEATH
    private void HandleManipulatedDeath(PlayerControl deadPlayer)
    {
        ClearManipulated(deadPlayer);
    }

    private void ClearManipulated(PlayerControl player, bool removeFromList = true)
    {
        remotes.GetValueOrDefault(player.PlayerId)?.ForEach(r => r?.Delete());
        if (removeFromList) manipulatedPlayers.Remove(player.PlayerId);
        expirationTimers.GetValueOrDefault(player.PlayerId)?.Finish(true);
        expirationTimers.Remove(player.PlayerId);
        remotes.Remove(player.PlayerId);
        if (MyPlayer.AmOwner) UpdateKillButton(UIManager.KillButton);
        else if (MyPlayer.IsModded()) Vents.FindRPC((uint)ModCalls.UpdateMastermind)?.Send([MyPlayer.OwnerId], CanManipulate);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream), key: "Manipulation Cooldown", name: ManipulationCooldown)
            .SubOption(sub => sub.KeyName("Manipulated Player Limit", ManipulatedPlayerLimit)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .AddIntRange(1, 5, 1, 0)
                .BindInt(i => manipulatedPlayerLimit = i)
                .Build())
            .SubOption(sub => sub.KeyName("Impostors Can See Manipulated", TranslationUtil.Colorize(ImpostorsCanSeeManipulated, RoleColor))
                .AddBoolean()
                .BindBool(b => impostorsCanSeeManipulated = b)
                .Build())
            .SubOption(sub => sub.KeyName("Time Until Suicide", TimeUntilSuicide)
                .Value(1f)
                .AddFloatRange(2.5f, 120, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(timeToKill.SetDuration)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(new IndirectKillCooldown(KillCooldown, () => CanManipulate))
            .OptionOverride(Override.KillCooldown, () => AUSettings.KillCooldown(), () => !CanManipulate)
            .RoleAbilityFlags(RoleAbilityFlag.UsesPet);

    [Localized(nameof(Mastermind))]
    internal static class MastermindTranslations
    {
        [Localized(nameof(ManipulateButtonText))]
        public static string ManipulateButtonText = "Manipulate";

        [Localized(nameof(ManipulatedText))]
        public static string ManipulatedText = "Manipulated!";

        [Localized(nameof(KillImploredText))]
        public static string KillImploredText = "You <b>MUST</b> Kill Someone In {0}\n(Use either your Kill or Pet button!)";

        [Localized(ModConstants.Options)]
        internal static class MastermindOptionTranslations
        {
            [Localized(nameof(ManipulationCooldown))]
            public static string ManipulationCooldown = "Manipulation Cooldown";

            [Localized(nameof(ManipulatedPlayerLimit))]
            public static string ManipulatedPlayerLimit = "Manipulated Player Limit";

            [Localized(nameof(ImpostorsCanSeeManipulated))]
            public static string ImpostorsCanSeeManipulated = "Impostors::0 Can See Manipulated";

            [Localized(nameof(TimeUntilSuicide))]
            public static string TimeUntilSuicide = "Time Until Suicide";
        }
    }
}