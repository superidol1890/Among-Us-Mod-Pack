using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotus;
using Lotus.Roles;
using Lotus.Roles.Operations;

namespace Lotus.Roles.Managers.Interfaces;

public interface IRoleManager
{
    public static RoleManager Current => ProjectLotus.GameModeManager.CurrentGameMode.RoleManager;

    public abstract CustomRole FallbackRole();

    public IEnumerable<CustomRole> AllCustomRoles();

    public void RegisterRole(CustomRole role);

    public IEnumerable<CustomRole> QueryMetadata(Predicate<object> metadataQuery) => AllCustomRoles().Where(rd => rd.Metadata.Select(m => m.Value).Any(obj => metadataQuery(obj)));

    public IEnumerable<CustomRole> QueryMetadata(string key, object value) => AllCustomRoles().Where(rd => rd.Metadata.Any(k => k.Key.Key == key && k.Value == value));

    public IEnumerable<CustomRole> QueryMetadata(object value) => QueryMetadata(md => md == value);

    public CustomRole GetRole(ulong assemblyRoleID, Assembly? assembly = null);

    public CustomRole GetRole(string globalRoleID);

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
        return AllCustomRoles().FirstOrDefault(r => QualifierFromRole(r) == qualifier, FallbackRole());
    }

    public static string QualifierFromRole(CustomRole role)
    {
        return $"{role.DeclaringAssembly.GetName().Name ?? "Unknown"}.{role.GetType().Name}.{role.EnglishRoleName}";
    }
}