using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Factions;
using Lotus.Factions.Neutrals;
using Lotus.GUI;
using Lotus.GUI.Counters;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Interfaces;
using Lotus.Options;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Factions.Impostors;
using Lotus.Logging;
using Lotus.Options.Roles;
using Lotus.Roles.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Networking.Interfaces;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using Lotus.API;
using VentLib.Networking;
using Lotus.Managers;
using System.Text.RegularExpressions;
using Lotus.Utilities;
using Lotus.Roles.RoleGroups.Crew;
using VentLib.Networking.RPC.Interfaces;

namespace Lotus.Roles;

[Localized("Roles")]
public abstract class CustomRole : AbstractBaseRole, IRpcSendable<CustomRole>
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(CustomRole));

    public string GlobalRoleID => $"${Addon?.UUID ?? 0}~{RoleID}";
    protected HashSet<Type> RelatedRoles = new();

    static CustomRole()
    {
        AbstractConstructors.Register(typeof(CustomRole), r => GlobalRoleManager.Instance.GetRoleFromId(r.ReadInt32()));
    }

    public virtual bool CanVent() => BaseCanVent && !RoleAbilityFlags.HasFlag(RoleAbilityFlag.CannotVent);

    public virtual void HandleDisconnect() { }

    public Relation Relationship(PlayerControl player) => Relationship(player.PrimaryRole());

    public virtual Relation Relationship(CustomRole role)
    {
        if (this.Faction is Neutral && role.Faction is Neutral)
            return Options.RoleOptions.NeutralOptions.NeutralTeamingMode switch
            {
                NeutralTeaming.All => Relation.FullAllies,
                NeutralTeaming.KillersNeutrals => RealRole.IsImpostor() && role.RealRole.IsImpostor() ? Relation.FullAllies : Relation.None,
                NeutralTeaming.SameRole => RelatedRoles.Contains(role.GetType()) ? Relation.FullAllies : Relation.None,
                _ => Relation.None
            };

        return Faction.Relationship(role);
    }

    private RemoteList<GameOptionOverride> currentOverrides = new();

    /// <summary>
    /// Utilized for "live" instances of the class AKA when the game is actually being played
    /// </summary>
    /// <returns>Shallow clone of this class (except for certain fields such as roleOptions being a deep clone)</returns>
    public CustomRole Instantiate(PlayerControl player)
    {
        CustomRole cloned = Clone();
        cloned.RelatedRoles.Add(this.GetType());
        cloned.MyPlayer = player;

        CreateInstanceBasedVariables();

        cloned.Setup(player);
        cloned.SetupUI2(player.NameModel());
        player.NameModel().Render(force: true);

        cloned.PostSetup();
        return cloned;
    }

    public CustomRole Clone()
    {
        CustomRole cloned = (CustomRole)this.MemberwiseClone();
        cloned.RoleSpecificGameOptionOverrides = new();
        cloned.currentOverrides = new();
        cloned.RelatedRoles = new HashSet<Type>(cloned.RelatedRoles);
        cloned.Modify(new RoleModifier(cloned));
        return cloned;
    }

    public bool IsEnabled() => this.Chance > 0 && (this.Count > 0 | this.RoleFlags.HasFlag(RoleFlag.RemoveRoleMaximum));

    /// <summary>
    /// Adds a GameOverride that continuously modifies this instances game options until removed
    /// </summary>
    /// <param name="optionOverride">Override to apply whenever SyncOptions is called</param>
    public Remote<GameOptionOverride> AddOverride(GameOptionOverride optionOverride) => currentOverrides.Add(optionOverride);

    public GameOptionOverride? GetOverride(Override overrideType) => GetRoleOverrides().LastOrDefault(o => o.Option == overrideType);
    /// <summary>
    /// Removes a continuous GameOverride
    /// </summary>
    /// <param name="optionOverride">Override to remove</param>
    protected void RemoveOverride(GameOptionOverride optionOverride) => currentOverrides.Remove(optionOverride);
    /// <summary>
    /// Removes a continuous GameOverride
    /// </summary>
    /// <param name="override">Override type to remove</param>
    protected void RemoveOverride(Override @override) => currentOverrides.RemoveAll(o => o.Option == @override);

    public virtual List<GameOptionOverride> GetRoleOverrides(IEnumerable<GameOptionOverride>? newOverrides = null)
    {
        DevLogger.Log($"(GetRoleOverrides) My player: {(MyPlayer == null ? "MyPlayer is null." : MyPlayer.name)}");
        List<GameOptionOverride> thisList = new(this.RoleSpecificGameOptionOverrides);

        thisList.AddRange(currentOverrides);
        thisList.AddRange(Game.MatchData.Roles.GetOverrides(MyPlayer.PlayerId));

        if (newOverrides != null) thisList.AddRange(newOverrides);

        return thisList;
    }

    // Useful for shorthand delegation
    public void SyncOptions() => SyncOptions(null);

    // ReSharper disable once MethodOverloadWithOptionalParameter
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public void SyncOptions(IEnumerable<GameOptionOverride>? newOverrides = null, bool official = false, bool parameterOverridesAreAbsolute = false)
    {
        if (MyPlayer == null || !AmongUsClient.Instance.AmHost) return;

        IEnumerable<GameOptionOverride>? overrides = newOverrides;
        if (!parameterOverridesAreAbsolute || overrides == null) overrides = GetRoleOverrides(newOverrides);
        if (this.RoleAbilityFlags.HasFlag(RoleAbilityFlag.UsesUnshiftTrigger))
        {
            overrides.Where(o => o.CanApply() && o.Option is Override.ShapeshiftDuration).ForEach(o => o.ForceValue = 0f);
        }

        IGameOptions modifiedOptions = DesyncOptions.GetModifiedOptions(overrides);
        if (official) RpcV3.Immediate(PlayerControl.LocalPlayer.NetId, RpcCalls.SyncSettings).Write(modifiedOptions).Send(MyPlayer.GetClientId());
        DesyncOptions.SyncToPlayer(modifiedOptions, MyPlayer);
    }

    public void Assign()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        bool isStartOfGame = Game.State is GameState.InIntro or GameState.InLobby;

        PlayerControl[] alliedPlayers = Players.GetPlayers().Where(p => Relationship(p) is Relation.FullAllies).ToArray();

        DevLogger.Log($"CustomRole.Assign({isStartOfGame}) for {MyPlayer.name} {ProjectLotus.AdvancedRoleAssignment}");
        // we can assign crewmate without worries as they should be crew for EVERYONE
        if (RealRole.IsCrewmate())
        {
            DevLogger.Log($"Real Role: {RealRole}");
            MyPlayer.RpcSetRole(RealRole, ProjectLotus.AdvancedRoleAssignment);

            if (!isStartOfGame) goto finishAssignment;

            Players.GetPlayers().ForEach(p => p.GetTeamInfo().AddPlayer(MyPlayer.PlayerId, MyPlayer.GetVanillaRole().IsImpostor()));

            goto finishAssignment;
        }

        // as impostor, its get a bit tricky.
        // we need to be wary of Noisemaker and Phantom since those cause issues if not replicated properly
        log.Trace($"Setting {MyPlayer.name} Role => {RealRole} | IsStartGame = {isStartOfGame}", "CustomRole::Assign");
        if (MyPlayer.IsHost()) MyPlayer.StartCoroutine(MyPlayer.CoSetRole(RealRole, ProjectLotus.AdvancedRoleAssignment));
        else RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetRole).Write((ushort)RealRole).Write(ProjectLotus.AdvancedRoleAssignment).Send(MyPlayer.GetClientId());

        log.Debug($"Player {MyPlayer.GetNameWithRole()} Allies: [{alliedPlayers.Select(p => p.name).Fuse()}]");
        HashSet<byte> alliedPlayerIds = alliedPlayers.Where(Faction.CanSeeRole).Select(p => p.PlayerId).ToHashSet();
        int[] alliedPlayerClientIds = alliedPlayers.Where(Faction.CanSeeRole).Select(p => p.GetClientId()).ToArray();

        PlayerControl[] crewmates = Players.GetPlayers().Where(p =>
        {
            DevLogger.Log($"Checking: {p.name} ({p.GetVanillaRole()})");
            return p != MyPlayer && p.GetVanillaRole().IsCrewmate();
        }).ToArray();
        int[] crewmateClientIds = crewmates.Select(p => p.GetClientId()).ToArray();
        log.Trace($"Current Crewmates: [{crewmates.Select(p => p.name).Fuse()}]");

        PlayerControl[] nonAlliedImpostors = Players.GetPlayers().Where(p => p.GetVanillaRole().IsImpostor()).Where(p => !alliedPlayerIds.Contains(p.PlayerId) && p.PlayerId != MyPlayer.PlayerId).ToArray();
        int[] nonAlliedImpostorClientIds = nonAlliedImpostors.Select(p => p.GetClientId()).ToArray();
        log.Trace($"Non Allied Impostors: [{nonAlliedImpostors.Select(p => p.name).Fuse()}]");

        RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetRole).Write((ushort)RealRole).Write(ProjectLotus.AdvancedRoleAssignment).SendInclusive(alliedPlayerClientIds);
        if (isStartOfGame) alliedPlayers.ForEach(p => p.GetTeamInfo().AddPlayer(MyPlayer.PlayerId, RealRole.IsImpostor()));

        RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetRole).Write((ushort)RoleTypes.Crewmate).Write(ProjectLotus.AdvancedRoleAssignment).SendInclusive(nonAlliedImpostorClientIds);
        if (isStartOfGame) nonAlliedImpostors.ForEach(p => p.GetTeamInfo().AddVanillaCrewmate(MyPlayer.PlayerId));
        else
        {
            // Make non-allied impostors crewmate for us.
            MassRpc massRpc = RpcV3.Mass(SendOption.Reliable);
            nonAlliedImpostors.ForEach(p => massRpc.Start(p.NetId, RpcCalls.SetRole).Write((ushort)RoleTypes.Crewmate).Write(ProjectLotus.AdvancedRoleAssignment).End());
            massRpc.Send(MyPlayer.GetClientId());
        }

        // This code exists to hopefully better split up the roles to cause less blackscreens
        RoleTypes splitRole = Faction is ImpostorFaction ? RealRole : RoleTypes.Crewmate;
        RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetRole).Write((ushort)splitRole).Write(ProjectLotus.AdvancedRoleAssignment).SendInclusive(crewmateClientIds);
        if (isStartOfGame) crewmates.ForEach(p =>
        {
            if (splitRole.IsImpostor()) p.GetTeamInfo().AddVanillaImpostor(MyPlayer.PlayerId);
            else p.GetTeamInfo().AddVanillaCrewmate(MyPlayer.PlayerId);
        });

        finishAssignment:

        ShowRoleToTeammates(alliedPlayers);

        // DevLogger.Log($"(HostCheck) Should See as Imp: {Relationship(PlayerControl.LocalPlayer) is Relation.FullAllies && Faction.CanSeeRole(PlayerControl.LocalPlayer)}");

        // This is for host. Should also fix player being set as impostor as host.
        if (RealRole.IsCrewmate()) MyPlayer.StartCoroutine(MyPlayer.CoSetRole(RealRole, ProjectLotus.AdvancedRoleAssignment));
        else if (Relationship(PlayerControl.LocalPlayer) is Relation.FullAllies && Faction.CanSeeRole(PlayerControl.LocalPlayer) || MyPlayer.IsHost()) MyPlayer.StartCoroutine(MyPlayer.CoSetRole(RealRole, ProjectLotus.AdvancedRoleAssignment));
        else MyPlayer.StartCoroutine(MyPlayer.CoSetRole(PlayerControl.LocalPlayer.GetVanillaRole().IsImpostor() ? RoleTypes.Crewmate : RoleTypes.Impostor, ProjectLotus.AdvancedRoleAssignment));

        SyncOptions(new GameOptionOverride[] { new(Override.KillCooldown, 0.1f) }, true);
        HudManager.Instance.SetHudActive(true);
    }

    public virtual void RefreshKillCooldown(PlayerControl? target = null, IEnumerable<GameOptionOverride>? overrides = null)
    {
        if (MyPlayer == null || !AmongUsClient.Instance.AmHost) return;
        if (target == null) target = MyPlayer;

        SyncOptions(overrides);
        if (MyPlayer.IsHost())
        {
            target.ShowFailedMurder();
            MyPlayer.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
            return;
        }
        RpcV3.Immediate(MyPlayer.NetId, RpcCalls.MurderPlayer).Write(target).Write((int)MurderResultFlags.FailedProtected).Send(MyPlayer.GetClientId());
    }

    // During intro scene to set team name and role info for non-modded clients and skip the rest.
    // Note: When Neutral is based on the Crewmate role then it is impossible to display the info for it
    // code taken from tohe
    public virtual string GetRoleIntroString()
    {
        if (MyPlayer.IsHost() || MyPlayer.IsModded()) return "";
        //Get role info font size based on the length of the role info
        int GetInfoSize(string RoleInfo)
        {
            RoleInfo = Regex.Replace(RoleInfo, "<[^>]*>", "");
            RoleInfo = Regex.Replace(RoleInfo, "{[^}]*}", "");

            var BaseFontSize = 200;
            int BaseFontSizeMin = 100;

            BaseFontSize -= 3 * RoleInfo.Length;
            if (BaseFontSize < BaseFontSizeMin)
                BaseFontSize = BaseFontSizeMin;
            return BaseFontSize;
        }

        string IconText = "<color=#ffffff>|</color>";
        string Font = "<font=\"VCR SDF\" material=\"VCR Black Outline\">";
        string SelfTeamName = $"<size=450%>{IconText} {Font}{this.Faction.Color.Colorize(this.Faction.Name())}</font> {IconText}</size><size=900%>\n \n</size>\r\n";
        string SelfRoleName = $"<size=185%>{Font}{this.RoleColor.Colorize(this.RoleName)}</font></size>";
        string SelfSubRolesName = Utils.GetSubRolesText(MyPlayer.PlayerId);
        string SeerRealName = MyPlayer.name;
        string SelfName = this.RoleColor.Colorize(SeerRealName);
        string RoleInfo = $"<size=25%>\n</size><size={GetInfoSize(this.Blurb)}%>{Font}{this.RoleColor.Colorize(this.Blurb)}</font></size>";
        string RoleNameUp = "<size=1350%>\n\n\n\n\n</size>";

        string finalString = $"{SelfTeamName}{SelfRoleName}{SelfSubRolesName}\r\n{RoleInfo}{RoleNameUp}";
        return finalString;
    }

    private void ShowRoleToTeammates(IEnumerable<PlayerControl> allies)
    {
        // Currently only impostors can show each other their roles
        RoleHolder roleHolder = MyPlayer.NameModel().GetComponentHolder<RoleHolder>();
        if (roleHolder.Count == 0)
        {
            log.Warn("Error Showing Roles to Allies. Role Component does not exist.", "CustomRole::ShowRoleToTeammates");
            return;
        }
        RoleComponent roleComponent = roleHolder[0];
        allies.Where(Faction.CanSeeRole).ForEach(a =>
        {
            log.Trace($"Showing Role {EnglishRoleName} to {a.name}", "ShowRoleToTeammates");
            roleComponent.AddViewer(a);
        });
    }

    private void SetupUI2(INameModel nameModel)
    {
        GameState[] gameStates = { GameState.InIntro, GameState.Roaming, GameState.InMeeting };

        if (this is ISubrole subrole && this.RoleFlags.HasFlag(RoleFlag.IsSubrole))
        {
            // if (subrole.Identifier() != null) nameModel.GetComponentHolder<SubroleHolder>().Add(new SubroleComponent(subrole, gameStates, viewers: MyPlayer));
            // else nameModel.GetComponentHolder<RoleHolder>().Add(new RoleComponent(this, gameStates, ViewMode.Additive, MyPlayer));
            nameModel.GetComponentHolder<SubroleHolder>().Add(new SubroleComponent(subrole, gameStates, viewers: MyPlayer));
        }
        else nameModel.GetComponentHolder<RoleHolder>().Add(new RoleComponent(this, gameStates, ViewMode.Overriden, MyPlayer));
        SetupUiFields(nameModel);
        SetupUiMethods(nameModel);
    }

    private void SetupUiFields(INameModel nameModel)
    {
        this.GetType().GetFields(AccessFlags.InstanceAccessFlags)
            .Where(f => f.GetCustomAttribute<UIComponent>() != null)
            .Reverse()
            .ForEach(f =>
            {
                UIComponent uiComponent = f.GetCustomAttribute<UIComponent>()!;
                object? value = f.GetValue(this);
                switch (uiComponent.Component)
                {
                    case UI.Name:
                        if (value is not string s) throw new ArgumentException($"Values for \"{nameof(UI.Name)}\" must be string. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<NameHolder>().Add(new NameComponent(s, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Role:
                        if (value is not CustomRole cr) throw new ArgumentException($"Values for \"{nameof(UI.Role)}\" must be {nameof(CustomRole)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<RoleHolder>().Add(new RoleComponent(cr, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    /*case UI.Subrole:
                        if (value is not Subrole sr) throw new ArgumentException($"Values for \"{nameof(UI.Subrole)}\" must be {nameof(Subrole)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<SubroleHolder>().Add(new SubroleComponent(sr, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;*/
                    case UI.Cooldown:
                        if (value is not Cooldown cd) throw new ArgumentException($"Values for \"{nameof(UI.Cooldown)}\" must be {nameof(Cooldown)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        log.Fatal($"Loading Cooldown Field: {cd} for {this}");
                        nameModel.GetComponentHolder<CooldownHolder>().Add(new CooldownComponent(cd, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Counter:
                        if (value is not ICounter counter) throw new ArgumentException($"Values for \"{nameof(UI.Counter)}\" must be {nameof(ICounter)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<CounterHolder>().Add(new CounterComponent(counter, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Indicator:
                        if (value is not string ind) throw new ArgumentException($"Values for \"{nameof(UI.Indicator)}\" must be string. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(ind, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Text:
                        if (value is not string txt) throw new ArgumentException($"Values for \"{nameof(UI.Indicator)}\" must be string. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<TextHolder>().Add(new TextComponent(txt, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Component: {uiComponent.Component} is not a valid component in role: {EnglishRoleName}");
                }
            });
    }

    private void SetupUiMethods(INameModel nameModel)
    {
        GameState[] gameStates = { GameState.InIntro, GameState.Roaming, GameState.InMeeting };
        this.GetType().GetMethods(AccessFlags.InstanceAccessFlags)
            .Where(m => m.GetCustomAttribute<UIComponent>() != null)
            .Reverse()
            .ForEach(m =>
            {
                UIComponent uiComponent = m.GetCustomAttribute<UIComponent>()!;
                if (m.GetParameters().Length > 0) throw new ConstraintException($"Methods marked by {nameof(UIComponent)} must have no parameters");

                Func<string> supplier = () => m.Invoke(this, null)?.ToString() ?? "N/A";
                switch (uiComponent.Component)
                {
                    case UI.Name:
                        nameModel.GetComponentHolder<NameHolder>().Add(new NameComponent(new LiveString(supplier), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Role:
                        nameModel.GetComponentHolder<RoleHolder>().Add(new RoleComponent(new LiveString(supplier), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Indicator:
                        nameModel.GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(new LiveString(supplier), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Text:
                        nameModel.GetComponentHolder<TextHolder>().Add(new TextComponent(new LiveString(supplier), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Cooldown:
                        object? CooldownSupplier() => m.Invoke(this, null);
                        var obj = CooldownSupplier();
                        if (obj is not Cooldown) throw new ArgumentException($"Values for \"{nameof(UI.Cooldown)}\" must be {nameof(Cooldown)}. (Got: {obj?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<CooldownHolder>().Add(new CooldownComponent(() => (Cooldown)m.Invoke(this, null)!, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Counter:
                        object? CounterSupplier() => m.Invoke(this, null);
                        var counterObj = CounterSupplier();
                        if (counterObj is string)
                        {
                            nameModel.GetComponentHolder<CounterHolder>().Add(new CounterComponent(new LiveString(() => (string)m.Invoke(this, null)!), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                            break;
                        }
                        if (counterObj is not ICounter) throw new ArgumentException($"Values for \"{nameof(UI.Counter)}\" must be {nameof(ICounter)}. (Got: {counterObj?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<CounterHolder>().Add(new CounterComponent(() => (ICounter)m.Invoke(this, null)!, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Subrole:
                    default:
                        throw new ArgumentOutOfRangeException($"Component: {uiComponent.Component} is not a valid component");
                }
            });
    }

    // will need to work on this an actually implement a serialize and deserialize.
    public CustomRole Read(MessageReader reader)
    {
        // string qualifier = reader.ReadString();
        // DevLogger.Log($"Qualifier: {qualifier}");
        // return null;
        // return ProjectLotus.RoleManager.GetRole(qualifier);
        return GlobalRoleManager.Instance.GetRoleFromId(reader.ReadInt32());
    }

    public void Write(MessageWriter writer)
    {
        /*string qualifier = ProjectLotus.RoleManager.GetIdentifier(this);
        DevLogger.Log($"Qualifier: {qualifier}");
        writer.Write(qualifier);*/
        writer.Write(GlobalRoleManager.Instance.GetRoleId(this));
    }


    public static bool operator ==(CustomRole? a, CustomRole? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(CustomRole? a, CustomRole? b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CustomRole role) return false;
        return role.GetType() == this.GetType();
    }
}