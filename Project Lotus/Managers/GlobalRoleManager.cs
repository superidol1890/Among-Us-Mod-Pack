using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lotus.GameModes;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using Lotus.Roles.Exceptions;
using Lotus.Roles.Managers.Interfaces;
using Lotus.Roles.Operations;
using VentLib.Options;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
namespace Lotus.Managers;
public class GlobalRoleManager
{
    public static readonly OptionManager RoleOptionManager = OptionManager.GetManager(file: "roleoptions.txt", managerFlags: OptionManagerFlags.SyncOverRpc);

    private readonly Dictionary<Assembly, Dictionary<ulong, CustomRole>> rolesbyAssembly = new();
    protected OrderedDictionary<string, CustomRole> OrderedCustomRoles { get; } = new();
    public static GlobalRoleManager Instance = null!;
    public GlobalRoleManager()
    {
        DevLogger.Log("Creating GlobalRoleManager.");
        Instance = this;
    }

    public IEnumerable<CustomRole> AllCustomRoles() => OrderedCustomRoles.GetValues();
    internal bool IsGlobal => true;

    public void RegisterRole(CustomRole role)
    {
        ulong roleID = role.RoleID;

        Dictionary<ulong, CustomRole> assemblyDefinitions = rolesbyAssembly.GetOrCompute(role.Assembly, () => new Dictionary<ulong, CustomRole>());
        if (!assemblyDefinitions.TryAdd(roleID, role)) throw new DuplicateRoleIdException(roleID, assemblyDefinitions[roleID], role);
        if (!OrderedCustomRoles.TryAdd(role.GlobalRoleID, role)) throw new DuplicateRoleIdException(role.GlobalRoleID, OrderedCustomRoles[role.GlobalRoleID], role);
    }

    public CustomRole GetRole(ulong assemblyRoleID, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        if (rolesbyAssembly[assembly].TryGetValue(assemblyRoleID, out CustomRole? role)) return role;
        throw new NoSuchRoleException($"Could not find role with ID \"{assemblyRoleID}\" from roles defined by: \"{assembly.FullName}\"");
    }

    public CustomRole GetRole(string globalRoleID)
    {
        if (OrderedCustomRoles.TryGetValue(globalRoleID, out CustomRole? role)) return role;
        throw new NoSuchRoleException($"Could not find role with global-ID \"{globalRoleID}\"");
    }

    public CustomRole GetRole(Type roleType) => AllCustomRoles().FirstOrDefault(d => (d as AbstractBaseRole).GetType() == roleType)!;

    public CustomRole GetRole<T>() where T : AbstractBaseRole => AllCustomRoles().FirstOrDefault(d => d.GetType() == typeof(T))!;
    public int GetRoleId(Type roleType)
    {
        List<CustomRole> AllRoles = AllCustomRoles().ToList();
        for (int i = 0; i < AllRoles.Count; i++)
            if (roleType == AllRoles[i].GetType())
                return i;
        return -1;
    }

    public CustomRole GetRoleFromName(string name) => AllCustomRoles().First(r => r.EnglishRoleName == name);

    public CustomRole GetRoleFromId(int id)
    {
        List<CustomRole> AllRoles = AllCustomRoles().ToList();
        if (id == -1) id = 0;
        return AllRoles[id];
    }

    public int GetRoleId(CustomRole role) => role == null ? 0 : GetRoleId(role.GetType());
    public CustomRole GetRoleFromType(Type roleType) => GetRoleFromId(GetRoleId(roleType));
    public CustomRole GetCleanRole(CustomRole role) => GetRoleFromId(GetRoleId(role));

    public CustomRole RoleFromQualifier(string qualifier)
    {
        return AllCustomRoles().FirstOrDefault(r => IRoleManager.QualifierFromRole(r) == qualifier, EmptyRole.Instance);
    }
}