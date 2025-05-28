using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.Factions;
using Lotus.Factions.Neutrals;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using Lotus.Roles.Debugger;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Managers.Interfaces;
using Lotus.Roles.Properties;
using Lotus.Roles.Subroles;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models.Backing;

// ReSharper disable once InconsistentNaming
internal class TUAllRoles
{
    public static string GetAllRoles(RoleSearchType searchType = RoleSearchType.AllRolesAndModifiers)
    {
        string? factionName = null;

        OrderedDictionary<string, List<CustomRole>> rolesByFaction = new();

        string FactionName(CustomRole roleDefinition)
        {
            if (roleDefinition.Metadata.GetOrEmpty(RoleProperties.Key).Compare(r => r.HasProperty(RoleProperty.IsModifier))) return FactionTranslations.Modifiers.Name;
            if (roleDefinition.Faction is not INeutralFaction) return roleDefinition.Faction.Name();

            SpecialType specialType = roleDefinition.Metadata.GetOrDefault(LotusKeys.AuxiliaryRoleType, SpecialType.None);

            return specialType is SpecialType.NeutralKilling ? FactionTranslations.NeutralKillers.Name : FactionTranslations.Neutral.Name;
        }

        bool Condition(CustomRole role)
        {
            if (role.GetType() == IRoleManager.Current.FallbackRole().GetType()) return false;
            if (searchType is RoleSearchType.OnlyModifiers) return role is ISubrole;
            return searchType is RoleSearchType.AllRolesAndModifiers || role is not ISubrole;
        }


        IRoleManager.Current.AllCustomRoles().ForEach(r => rolesByFaction.GetOrCompute(FactionName(r), () => new List<CustomRole>()).Add(r));

        string text = "";

        List<string> roleNames = new();
        rolesByFaction.GetValues().SelectMany(s => s).Where(Condition).ForEach(r =>
        {
            string fName = FactionName(r);
            if (factionName != fName)
            {
                text += roleNames.Fuse();
                roleNames = new List<string>();
                if (factionName == FactionTranslations.Modifiers.Name) text += $"\n★ {factionName} ★\n";
                else text += $"\n\n★ {fName} ★\n";
                factionName = fName;
            }

            roleNames.Add(r.RoleName);
        });

        text += roleNames.Fuse();

        return text.TrimStart('\n');
    }
}

public enum RoleSearchType
{
    AllRolesAndModifiers,
    OnlyModifiers,
    OnlyRoles
}