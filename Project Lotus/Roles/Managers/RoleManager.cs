using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotus;
using Lotus.GameModes;
using Lotus.Roles;
using Lotus.Roles.Operations;
using Lotus.Roles.Managers.Interfaces;
using Lotus.Managers;

namespace Lotus.Roles.Managers;
public abstract class RoleManager : IRoleManager
{
    public abstract CustomRole FallbackRole();
    public abstract RoleHolder RoleHolder { get; }

    public abstract IEnumerable<CustomRole> AllCustomRoles();

    public virtual void RegisterRole(CustomRole role)
    {
        GlobalRoleManager.Instance.RegisterRole(role);
    }

    public IEnumerable<CustomRole> QueryMetadata(Predicate<object> metadataQuery) => AllCustomRoles().Where(rd => rd.Metadata.Select(m => m.Value).Any(obj => metadataQuery(obj)));

    public IEnumerable<CustomRole> QueryMetadata(string key, object value) => AllCustomRoles().Where(rd => rd.Metadata.Any(k => k.Key.Key == key && k.Value == value));

    public IEnumerable<CustomRole> QueryMetadata(object value) => QueryMetadata(md => md == value);

    public abstract CustomRole GetRole(ulong assemblyRoleID, Assembly? assembly = null);

    public abstract CustomRole GetRole(string globalRoleID);

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