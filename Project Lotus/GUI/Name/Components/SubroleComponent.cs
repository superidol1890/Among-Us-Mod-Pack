using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.GUI.Name.Components;

public class SubroleComponent : SimpleComponent
{
    private AbstractBaseRole? subrole;

    public SubroleComponent(ISubrole subrole, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.subrole = subrole as AbstractBaseRole;
        this.SetMainText(new LiveString(subrole.Identifier() ?? "", this.subrole?.RoleColor));
    }

    public SubroleComponent(ISubrole subrole, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : this(subrole, [gameState], viewMode, viewers)
    {
        this.subrole = subrole as AbstractBaseRole;
        this.SetMainText(new LiveString(subrole.Identifier() ?? "", this.subrole?.RoleColor));
    }

    public override string GenerateText(GameState state)
    {
        if (subrole == null) return base.GenerateText(state);
        string newString = Prefixes.Fuse("") + subrole.ColoredRoleName() + Suffixes.Fuse("");
        size.IfPresent(s => newString = TextUtils.ApplySize(s, newString));
        return newString;
    }

    public string GenerateIdentifier(GameState state)
    {
        if (subrole == null) return base.GenerateText(state);
        string newString = ((ISubrole)subrole).Identifier() ?? "";
        if (newString != "") newString = subrole.RoleColor.Colorize(newString);
        size.IfPresent(s => newString = TextUtils.ApplySize(s, newString));
        return newString;
    }
}