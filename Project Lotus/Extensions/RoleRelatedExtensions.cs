using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.Roles;
using VentLib.Utilities;

namespace Lotus.Extensions;

public static class RoleRelatedExtensions
{
    public static CustomRole? GetPrimaryRole(this NetworkedPlayerInfo? playerInfo)
    {
        return playerInfo == null ? null : Game.MatchData.Roles.GetMainRole(playerInfo.PlayerId);
    }

    public static IEnumerable<CustomRole> GetAllRoleDefinitions(this PlayerControl player) => Game.CurrentGameMode.MatchData.Roles.GetRoleDefinitions(player.PlayerId);

    public static string ColoredRoleName(this CustomRole role) => role.RoleColor.Colorize(role.RoleName);
}