using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadReloaded.Roles.Outcast;

public interface IOutcastRole : ICustomRole
{
    ModdedRoleTeams ICustomRole.Team => ModdedRoleTeams.Custom;
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => new("Outcast Roles", Color.gray);
    TeamIntroConfiguration? ICustomRole.IntroConfiguration => new(Color.gray, "OUTCAST", "You are an Outcast. You do not have a team.");
}