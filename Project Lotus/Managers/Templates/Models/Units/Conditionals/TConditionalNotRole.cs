using System.Collections.Generic;
using System.Linq;
using Lotus.Extensions;
using Lotus.Roles;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalNotRole : StringListConditionalUnit
{
    private HashSet<string>? rolesLower;

    public TConditionalNotRole(object input) : base(input)
    {
    }

    public override bool Evaluate(object? data)
    {
        return data is not PlayerControl player || VerifyNotRole(player);
    }

    private bool VerifyNotRole(PlayerControl? player)
    {
        if (player == null) return true;
        rolesLower ??= Values.Select(r => r.ToLower()).ToHashSet();
        return !player.GetAllRoleDefinitions().Any(VerifyRole);
    }

    private bool VerifyRole(CustomRole role)
    {
        return rolesLower == null || rolesLower.Contains(role.RoleName);
    }
}