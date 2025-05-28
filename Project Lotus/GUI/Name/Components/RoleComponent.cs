using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.Extensions;
using System.Text.RegularExpressions;
using VentLib.Utilities;
using Lotus.Logging;

namespace Lotus.GUI.Name.Components;

public class RoleComponent : SimpleComponent
{
    private CustomRole role;

    public RoleComponent(CustomRole role, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(LiveString.Empty, gameStates, viewMode, viewers)
    {
        this.role = role;
        this.SetMainText(new LiveString(role.ColoredRoleName));
    }

    public RoleComponent(CustomRole role, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : this(role, gameStates, viewMode, viewers.ToList)
    {
    }

    public RoleComponent(CustomRole role, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : this(role, [gameState], viewMode, viewers)
    {
    }

    public RoleComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public RoleComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }
}