using System.Collections.Generic;
using System.Linq;
using Lotus.Roles;
using Lotus.Roles.Managers.Interfaces;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalEnabledRoles : StringListConditionalUnit
{
    private HashSet<string>? rolesLower;

    public TConditionalEnabledRoles(object input) : base(input)
    {
    }

    public override bool Evaluate(object? data)
    {
        rolesLower ??= Values.Select(r => r.ToLower()).ToHashSet();
        HashSet<string> gameEnabledRoleCache = new();
        bool iterated = false;
        foreach (string role in rolesLower)
        {
            if (gameEnabledRoleCache.Contains(role)) return true;
            if (iterated) continue;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (CustomRole roleDefinition in IRoleManager.Current.AllCustomRoles())
            {
                if (!roleDefinition.IsEnabled()) continue;
                gameEnabledRoleCache.Add(roleDefinition.RoleName.ToLower());
                if (gameEnabledRoleCache.Contains(role)) return true;
            }

            iterated = true;
        }

        return false;
    }
}