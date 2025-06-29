using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MiraAPI.Roles;

/// <summary>
/// A utility class to get the instance of a custom role.
/// </summary>
/// <typeparam name="T">The role you are trying to access.</typeparam>
public static class CustomRoleSingleton<T> where T : ICustomRole
{
    private static T? _instance;

    /// <summary>
    /// Gets the instance of the role.
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is a utility class to get the instance of a custom role.")]
    public static T Instance => _instance ??= CustomRoleManager.CustomRoles.Values.OfType<T>().Single();
}
