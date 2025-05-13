extern alias JBAnnotations;
using System.Collections.Generic;
using Lotus.Roles.Distribution;
using JBAnnotations::JetBrains.Annotations;
using Lotus.Extensions;
using Lotus.GameModes.Standard.Distributions;
using Lotus.Options;
using Lotus.Roles;
using System.Linq;
using Lotus.Utilities;
using Lotus.Roles.RoleGroups.Vanilla;
using VentLib.Utilities.Extensions;
using Lotus.API.Player;
using Lotus.GameModes.Standard.Lotteries;
using Lotus.Roles.Interfaces;
using Lotus.Factions.Impostors;
using Lotus.Roles.Builtins;
using Lotus.Roles.Subroles;
using Lotus.Patches;

namespace Lotus.GameModes.Standard.Distributions;

public class StandardRoleAssignment
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(StandardRoleAssignment));
    public static StandardRoleAssignment Instance = null!;
    public StandardRoleAssignment()
    {
        Instance = this;
    }

    private static List<IAdditionalAssignmentLogic> _additionalAssignmentLogics = new();

    [UsedImplicitly]
    public static void AddAdditionalAssignmentLogic(IAdditionalAssignmentLogic logic) => _additionalAssignmentLogics.Add(logic);

    public void AssignRoles(List<PlayerControl> allPlayers)
    {
        List<PlayerControl> unassignedPlayers = new(allPlayers);
        unassignedPlayers.Shuffle();

        RoleDistribution roleDistribution = GeneralOptions.GameplayOptions.OptimizeRoleAssignment
            ? OptimizeRoleAlgorithm.OptimizeDistribution()
            : OptimizeRoleAlgorithm.NonOptimizedDistribution();

        log.Debug("Assigning Roles..");

        DoNameBasedAssignment(unassignedPlayers);

        // ASSIGN IMPOSTOR ROLES
        log.Debug("Assigning Impostor Roles");
        RunAdditionalAssignmentLogic(allPlayers, unassignedPlayers, 1);
        ImpostorLottery impostorLottery = new();
        int impostorCount = 0;
        int madmateCount = 0;

        while ((impostorCount < roleDistribution.Impostors || madmateCount < roleDistribution.MinimumMadmates) && unassignedPlayers.Count > 0)
        {
            CustomRole role = impostorLottery.Next();
            if (role.GetType() == typeof(Impostor) && impostorLottery.HasNext()) continue;

            if (role.Faction is Madmates)
            {
                if (madmateCount >= roleDistribution.MaximumMadmates) continue;
                StandardGameMode.Instance.Assign(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
                madmateCount++;
                if (RoleOptions.MadmateOptions.MadmatesTakeImpostorSlots) impostorCount++;
                continue;
            }

            if (impostorCount >= roleDistribution.Impostors)
            {
                if (!impostorLottery.HasNext()) break;
                continue;
            }

            StandardGameMode.Instance.Assign(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
            impostorCount++;
        }

        // =====================

        // ASSIGN NEUTRAL KILLING ROLES
        log.Debug("Assigning Neutral Killing Roles");
        RunAdditionalAssignmentLogic(allPlayers, unassignedPlayers, 2);
        NeutralKillingLottery neutralKillingLottery = new();
        int nkRoles = 0;
        int loops = 0;
        while (unassignedPlayers.Count > 0 && nkRoles < roleDistribution.MaximumNeutralKilling)
        {
            if (loops > 0 && nkRoles >= roleDistribution.MinimumNeutralKilling) break;
            CustomRole role = neutralKillingLottery.Next();
            if (role is IllegalRole)
            {
                if (nkRoles >= roleDistribution.MinimumNeutralKilling || loops >= 10) break;
                loops++;
                if (!neutralKillingLottery.HasNext())
                    neutralKillingLottery = new NeutralKillingLottery(); // Refresh the lottery again to fulfill the minimum requirement
                continue;
            }
            StandardGameMode.Instance.Assign(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
            nkRoles++;
        }

        // --------------------------

        // ASSIGN NEUTRAL PASSIVE ROLES
        log.Debug("Assigning Neutral Passive Roles");
        RunAdditionalAssignmentLogic(allPlayers, unassignedPlayers, 3);
        NeutralLottery neutralLottery = new();
        int neutralRoles = 0;
        loops = 0;
        while (unassignedPlayers.Count > 0 && neutralRoles < roleDistribution.MaximumNeutralPassive)
        {
            if (loops > 0 && neutralRoles >= roleDistribution.MinimumNeutralPassive) break;
            CustomRole role = neutralLottery.Next();
            if (role is IllegalRole)
            {
                if (neutralRoles >= roleDistribution.MinimumNeutralPassive || loops >= 10) break;
                loops++;
                if (!neutralLottery.HasNext())
                    neutralLottery = new NeutralLottery(); // Refresh the lottery again to fulfill the minimum requirement
                continue;
            }
            StandardGameMode.Instance.Assign(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
            neutralRoles++;
        }

        // =====================

        // ASSIGN CREWMATE ROLES
        log.Debug("Assigning Crewmate Roles");
        RunAdditionalAssignmentLogic(allPlayers, unassignedPlayers, 4);
        CrewmateLottery crewmateLottery = new();
        while (unassignedPlayers.Count > 0)
        {
            CustomRole role = crewmateLottery.Next();
            if (role.GetType() == typeof(Crewmate) && crewmateLottery.HasNext()) continue;
            StandardGameMode.Instance.Assign(unassignedPlayers.PopRandom(), role);
        }

        // ====================

        // ASSIGN SUB-ROLES
        AssignSubroles(allPlayers);
        // ================

        log.Debug("Finishing up...");
        RunAdditionalAssignmentLogic(allPlayers, unassignedPlayers, 5);
        log.Debug("Finished assigning roles!");
    }

    private void DoNameBasedAssignment(List<PlayerControl> unassignedPlayers)
    {
        if (!GeneralOptions.DebugOptions.NameBasedRoleAssignment) return;
        log.Debug("Doing Name Based Role Assigment!");
        int j = 0;
        while (j < unassignedPlayers.Count)
        {
            PlayerControl player = unassignedPlayers[j];
            CustomRole? role = StandardGameMode.Instance.RoleManager.RoleHolder.AllRoles.FirstOrDefault(r => r.RoleName.RemoveHtmlTags().ToLower().StartsWith(player.name.ToLower() ?? "HEHXD"));
            if (role != null && role.GetType() != typeof(Crewmate))
            {
                StandardGameMode.Instance.Assign(player, role);
                unassignedPlayers.Pop(j);
            }
            else j++;
        }
    }

    private void AssignSubroles(List<PlayerControl> allPlayers)
    {
        if (RoleOptions.SubroleOptions.ModifierLimits == 0) return; // no modifiers
        log.Debug("Assigning Subroles...");
        SubRoleLottery subRoleLottery = new();

        int evenDistribution = RoleOptions.SubroleOptions.EvenlyDistributeModifiers ? 0 : 9999;

        bool CanAssignTo(PlayerControl player)
        {
            int count = player.GetSubroles().Count;
            if (count > evenDistribution) return false;
            return RoleOptions.SubroleOptions.UncappedModifiers || count < RoleOptions.SubroleOptions.ModifierLimits;
        }

        while (subRoleLottery.HasNext())
        {
            CustomRole role = subRoleLottery.Next();
            if (role is IllegalRole) continue;
            if (role is IRoleCandidate candidate)
                if (candidate.ShouldSkip()) continue;
            CustomRole variant = role is Subrole sr ? IVariantSubrole.PickAssignedRole(sr) : IVariableRole.PickAssignedRole(role);
            List<PlayerControl> players = Players.GetAllPlayers().Where(CanAssignTo).ToList();
            if (players.Count == 0)
            {
                evenDistribution++;
                if (!RoleOptions.SubroleOptions.UncappedModifiers && evenDistribution >= RoleOptions.SubroleOptions.ModifierLimits) break;
                players = Players.GetAllPlayers().Where(p => p.GetSubroles().Count <= evenDistribution).ToList(); ;
                if (players.Count == 0) break;
            }
            log.Debug($"testing role {role.EnglishRoleName}");

            bool assigned = false;
            while (players.Count > 0 && !assigned)
            {
                PlayerControl victim = players.PopRandom();
                if (victim.GetSubroles().Any(r => r.GetType() == variant.GetType())) continue;
                if (variant is ISubrole subrole && !(assigned = subrole.IsAssignableTo(victim))) continue;
                StandardGameMode.Instance.Assign(victim, variant, false);
            }
        }
    }

    private void RunAdditionalAssignmentLogic(List<PlayerControl> allPlayers, List<PlayerControl> unassignedPlayers, int stage)
        => _additionalAssignmentLogics.ForEach(logic => logic.AssignRoles(allPlayers, unassignedPlayers, stage));
}