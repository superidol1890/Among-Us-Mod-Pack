using System.Linq;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace Lotus.GameModes.Standard.Lotteries;

public class SubRoleLottery : RoleLottery
{
    public SubRoleLottery() : base(StandardRoles.Instance.Special.IllegalRole)
    {
        StandardRoles.Instance.AllRoles.Where(r => r.RoleFlags.HasFlag(RoleFlag.IsSubrole)).ForEach(r => AddRole(r));
    }
}