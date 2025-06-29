using System;

namespace MiraAPI.Roles;

/// <summary>
/// Utility class to get the ID of a role.
/// </summary>
public static class RoleId
{
    /// <summary>
    /// Gets the ID of a role.
    /// </summary>
    /// <typeparam name="T">The type of the role.</typeparam>
    /// <returns>The role ID as a ushort.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the role is not registered.</exception>
    public static ushort Get<T>() where T : ICustomRole
    {
        if (!CustomRoleManager.RoleIds.TryGetValue(typeof(T), out var roleId))
        {
            throw new InvalidOperationException($"Role {typeof(T)} is not registered");
        }

        return roleId;
    }

    /// <summary>
    /// Gets the ID of a role.
    /// </summary>
    /// <param name="type">The type of the role.</param>
    /// <returns>The ID as a ushort.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the role is not registered.</exception>
    public static ushort Get(Type type)
    {
        if (!CustomRoleManager.RoleIds.TryGetValue(type, out var roleId))
        {
            throw new InvalidOperationException($"Role {type} is not registered");
        }

        return roleId;
    }
}
