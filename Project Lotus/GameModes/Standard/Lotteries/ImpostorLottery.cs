using System.Linq;
using Lotus.Factions.Impostors;
using Lotus.Managers;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace Lotus.GameModes.Standard.Lotteries;

public class ImpostorLottery : RoleLottery
{
    public ImpostorLottery() : base(StandardRoles.Instance.Static.Impostor)
    {
        StandardRoles.Instance.AllRoles.Where(r => r.Faction is ImpostorFaction).ForEach(r => AddRole(r));
    }
}