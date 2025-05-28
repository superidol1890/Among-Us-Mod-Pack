using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lotus.Options;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.NeutralKilling;
using UnityEngine;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.Roles.Internals;
using Lotus.Utilities;
using Random = UnityEngine.Random;
using VentLib.Utilities;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Lotus.GameModes.CTF;
using VentLib.Utilities.Collections;
using Lotus.GUI.Name.Components;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name;
using Lotus.API.Player;
using Lotus.API;
using Lotus.RPC.CustomObjects;
using Lotus.RPC.CustomObjects.Builtin;
using Lotus.Logging;
using Lotus.GameModes.Colorwars.Factions;

namespace Lotus.Roles.RoleGroups.CTF;

public class Striker : NeutralKillingBase
{
    private Cooldown noticeTimer = null!;
    private Cooldown reviveTimer = null!;
    private Cooldown gameTimer = null!;

    [UIComponent(UI.Counter)]
    public string GameTimerText() => Color.white.Colorize($"\n{gameTimer}{GeneralOptionTranslations.SecondsSuffix}");

    [UIComponent(UI.Counter)]
    public string ReviveCooldownText() => reviveTimer.IsReady() ? "" : Color.yellow.Colorize(Translations.ReviveCooldown.Formatted(reviveTimer));

    [UIComponent(UI.Text)]
    public string CurrentScore() => $"{Color.red.Colorize(CTFGamemode.Team0Score.ToString())} {Color.white.Colorize("|")} {Color.blue.Colorize(CTFGamemode.Team1Score.ToString())}";

    [UIComponent(UI.Text)]
    public string FlagGrabNotice() => noticeTimer.IsReady() ? "" : Color.yellow.Colorize(Translations.GrabbedFlag) + "\n";

    [NewOnSetup] private List<Remote<IndicatorComponent>> arrowComponents = null!;
    private Remote<Overrides.GameOptionOverride>? speedOverride;
    private Remote<IndicatorComponent>? warningIndicator;


    protected override void PostSetup()
    {
        KillCooldown = ExtraGamemodeOptions.CaptureOptions.KillCooldown * 2;
    }

    [RoleAction(LotusActionType.RoundStart)]
    private void RoundStart()
    {
        // Here we start all cooldowns and setup everything.
        reviveTimer.SetDuration(ExtraGamemodeOptions.CaptureOptions.ReviveDuration);
        gameTimer.SetDuration(ExtraGamemodeOptions.CaptureOptions.GameLength);

        gameTimer.Start();

        if (CTFGamemode.SpawnLocations == null)
        {
            if (ShipStatus.Instance is AirshipStatus) CTFGamemode.SpawnLocations = [RandomSpawn.AirshipLocations["Cockpit"], RandomSpawn.AirshipLocations["CargoBay"]];
            else CTFGamemode.SpawnLocations = ShipStatus.Instance.Type switch
            {
                ShipStatus.MapType.Ship => [RandomSpawn.SkeldLocations["Reactor"], RandomSpawn.SkeldLocations["Navigation"]],
                ShipStatus.MapType.Hq => [RandomSpawn.MiraLocations["Launchpad"], RandomSpawn.MiraLocations["Cafeteria"]],
                ShipStatus.MapType.Pb => [RandomSpawn.PolusLocations["BoilerRoom"], RandomSpawn.PolusLocations["Laboratory"]],
                _ => throw new ArgumentOutOfRangeException(ShipStatus.Instance.Type.ToString())
            };
            CTFGamemode.RedFlag = new RedFlag(CTFGamemode.SpawnLocations[0]);
            CTFGamemode.BlueFlag = new BlueFlag(CTFGamemode.SpawnLocations[1]);
        }

        Utils.Teleport(MyPlayer.NetTransform, CTFGamemode.SpawnLocations[MyPlayer.cosmetics.bodyMatProperties.ColorId]);
    }

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(LotusActionType.Interaction)]
    private void FakeDie(ActionHandle handle)
    {
        handle.Cancel(); // Stops me from dying and does RpcMark.
        if (reviveTimer.NotReady()) return;

        Utils.Teleport(MyPlayer.NetTransform, new Vector2(Random.RandomRange(5000, 99999), Random.RandomRange(5000, 99999)));
        reviveTimer.StartThenRun(RevivePlayer);

        PutDownFlag(false);
    }

    [RoleAction(LotusActionType.OnPet)]
    private void OnTouchFlag()
    {
        bool atRedFlag = RoleUtils.GetPlayersWithinDistance(CTFGamemode.SpawnLocations[0], 2f).Any(p => p.PlayerId == MyPlayer.PlayerId);
        bool atBlueFlag = RoleUtils.GetPlayersWithinDistance(CTFGamemode.SpawnLocations[1], 2f).Any(p => p.PlayerId == MyPlayer.PlayerId);
        if (!atRedFlag && !atBlueFlag) return;
        bool isRedTeam = MyPlayer.cosmetics.bodyMatProperties.ColorId == 0;

        if (isRedTeam)
        {
            if (atRedFlag) PutDownFlag(true);
            if (atBlueFlag)
            {
                // I am red team at blue's flag.
                if (CTFGamemode.Team1FlagCarrier != byte.MaxValue) return;
                GrabFlag(isRedTeam);
            }
        }
        else
        {
            if (atBlueFlag) PutDownFlag(true);
            if (atRedFlag)
            {
                // I am blue team at red's flag.
                if (CTFGamemode.Team0FlagCarrier != byte.MaxValue) return;
                GrabFlag(isRedTeam);
            }
        }
    }

    [RoleAction(LotusActionType.VentEntered)]
    private void CheckIfCarrying(ActionHandle handle)
    {
        if (ExtraGamemodeOptions.CaptureOptions.CarryingCanVent) return;

        if (CTFGamemode.Team0FlagCarrier == MyPlayer.PlayerId) handle.Cancel();
        else if (CTFGamemode.Team1FlagCarrier == MyPlayer.PlayerId) handle.Cancel();
    }

    private void RevivePlayer()
    {
        Utils.Teleport(MyPlayer.NetTransform, CTFGamemode.SpawnLocations[MyPlayer.cosmetics.bodyMatProperties.ColorId]);
    }

    private void PutDownFlag(bool awardPoint)
    {
        if (CTFGamemode.Team0FlagCarrier == MyPlayer.PlayerId)
        {
            CTFGamemode.Team0FlagCarrier = byte.MaxValue;
            if (awardPoint) CTFGamemode.Team1Score += 1;
        }
        else if (CTFGamemode.Team1FlagCarrier == MyPlayer.PlayerId)
        {
            CTFGamemode.Team1FlagCarrier = byte.MaxValue;
            if (awardPoint) CTFGamemode.Team0Score += 1;
        }
        else
        {
            return;
        }
        warningIndicator?.Delete();
        speedOverride?.Delete();
        arrowComponents.ForEach(c => c.Delete());
        arrowComponents.Clear();
        noticeTimer.Finish();
        MyPlayer.SyncAll();
    }

    private void GrabFlag(bool isRedTeam)
    {
        if (isRedTeam)
        {
            CTFGamemode.Team1FlagCarrier = MyPlayer.PlayerId;
        }
        else
        {
            CTFGamemode.Team0FlagCarrier = MyPlayer.PlayerId;
        }

        warningIndicator = MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(new LiveString("⚠", RoleColor), Game.InGameStates));
        Color myColor = MyPlayer.PrimaryRole().RoleColor;
        Players.GetAllPlayers().ForEach(p =>
        {
            if (MyPlayer == p) return;
            LiveString liveString = new(() => RoleUtils.CalculateArrow(p, MyPlayer, myColor));
            var remote = p.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: p));
            arrowComponents.Add(remote);
        });
        noticeTimer.Start(4);
        speedOverride = Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, new(Overrides.Override.PlayerSpeedMod, AUSettings.PlayerSpeedMod() * ExtraGamemodeOptions.CaptureOptions.CarryingSpeedMultiplier));
        MyPlayer.SyncAll();
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier)
        .RoleFlags(RoleFlag.DontRegisterOptions | RoleFlag.Hidden)
        .RoleAbilityFlags(RoleAbilityFlag.CannotSabotage)
        .IntroSound(AmongUs.GameOptions.RoleTypes.Shapeshifter)
        .CanVent(ExtraGamemodeOptions.CaptureOptions.CanVent)
        .VanillaRole(AmongUs.GameOptions.RoleTypes.Impostor)
        .Faction(ColorFaction.Instance)
        .RoleColor(Color.white);

    [Localized(nameof(Striker))]
    public class Translations
    {
        [Localized(nameof(ReviveCooldown))] public static string ReviveCooldown = "Reviving In: {0}";
        [Localized(nameof(GrabbedFlag))] public static string GrabbedFlag = "You grabbed the flag! The opposing team has arrows to your location!";
    }
}