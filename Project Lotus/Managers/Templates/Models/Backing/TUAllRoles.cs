using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.Factions.Neutrals;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using Lotus.Roles.Debugger;
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
    public static string GetAllRoles(bool allowSubroles, bool onlyModifiers = false)
    {
        string? factionName = null;

        OrderedDictionary<string, List<CustomRole>> rolesByFaction = new();

        string FactionName(CustomRole roleDefinition)
        {
            if (roleDefinition.Metadata.GetOrEmpty(RoleProperties.Key).Compare(r => r.HasProperty(RoleProperty.IsModifier))) return "Modifiers";
            if (roleDefinition.Faction is not Neutral) return roleDefinition.Faction.Name();

            SpecialType specialType = roleDefinition.Metadata.GetOrDefault(LotusKeys.AuxiliaryRoleType, SpecialType.None);

            return specialType is SpecialType.NeutralKilling ? "Neutral Killers" : "Neutral";
        }

        bool Condition(CustomRole role)
        {
            if (role is EmptyRole) return false;
            if (onlyModifiers && role is Subrole) return true;
            if (onlyModifiers) return false;
            if (allowSubroles) return true;
            return role is not Subrole;
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
                if (factionName == "Modifiers") text += $"\n★ {factionName} ★\n";
                else text += $"\n\n★ {fName} ★\n";
                factionName = fName;
            }

            roleNames.Add(r.RoleName);
        });

        text += roleNames.Fuse();

        return text.TrimStart('\n');
    }
}