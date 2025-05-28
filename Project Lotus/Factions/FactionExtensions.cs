using Lotus.Factions.Interfaces;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles.Operations;

namespace Lotus.Factions;

public static class FactionExtensions
{
    public static Relation Relationship(this PlayerControl player, PlayerControl other) => RoleOperations.Current.Relationship(player, other);

    public static Relation Relationship(this PlayerControl player, CustomRole other) => RoleOperations.Current.Relationship(player.PrimaryRole(), other);

    public static Relation Relationship(this PlayerControl player, IFaction faction) => RoleOperations.Current.Relationship(player.PrimaryRole(), faction);
}