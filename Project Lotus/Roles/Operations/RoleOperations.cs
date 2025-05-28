using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.Runtime.Remoting;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.GameModes;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Rewired.Utils;
using UnityEngine.ResourceManagement.Util;
using VentLib.Utilities.Collections;

namespace Lotus.Roles.Operations;

// ReSharper disable once InconsistentNaming
public interface RoleOperations : IRoleComponent
{
    public static RoleOperations Current => ProjectLotus.GameModeManager.CurrentGameMode.RoleOperations;
    public abstract GameMode? ParentGameMode { get; }

    public Relation Relationship(CustomRole source, CustomRole comparison);

    public Relation Relationship(CustomRole source, IFaction comparison);

    public Relation Relationship(PlayerControl source, PlayerControl comparison) => Relationship(source.PrimaryRole(), comparison.PrimaryRole());

    public void SyncOptions(PlayerControl target) => SyncOptions(target, Game.MatchData.Roles.GetRoleDefinitions(target.PlayerId));

    public void SyncOptions(CustomRole definition) => SyncOptions(definition.MyPlayer, definition);

    public void SyncOptions(PlayerControl target, params CustomRole[] definitions) => SyncOptions(target, definitions, null);

    public void SyncOptions(PlayerControl target, IEnumerable<CustomRole> definitions, IEnumerable<GameOptionOverride>? overrides = null, bool deepSet = false);

    public ActionHandle Trigger(LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters);

    public ActionHandle Trigger(LotusActionType action, PlayerControl? source, params object[] parameters) => Trigger(action, source, ActionHandle.NoInit(), parameters);

    public ActionHandle TriggerFor(IEnumerable<CustomRole> recipients, LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters);

    public ActionHandle TriggerFor(IEnumerable<CustomRole> recipients, LotusActionType action, PlayerControl? source, params object[] parameters) => TriggerFor(recipients, action, source, ActionHandle.NoInit(), parameters);

    public ActionHandle TriggerFor(IEnumerable<PlayerControl> players, LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters) => TriggerFor(players.SelectMany(p => p.GetAllRoleDefinitions()), action, source, handle, parameters);

    public ActionHandle TriggerFor(IEnumerable<PlayerControl> players, LotusActionType action, PlayerControl? source, params object[] parameters) => TriggerFor(players, action, source, ActionHandle.NoInit(), parameters);

    public ActionHandle TriggerFor(PlayerControl player, LotusActionType action, PlayerControl? source, params object[] parameters) => TriggerFor(new List<PlayerControl> { player }.ToArray(), action, source, parameters);

    public ActionHandle TriggerFor(PlayerControl player, LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters) => TriggerFor(new List<PlayerControl> { player }.ToArray(), action, source, handle, parameters);

    public ActionHandle TriggerForAll(LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters) => TriggerFor(Players.GetPlayers(), action, source, handle, parameters);

    public ActionHandle TriggerForAll(LotusActionType action, PlayerControl? source, params object[] parameters) => TriggerFor(Players.GetPlayers(), action, source, parameters);
}