using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.Roles;

namespace Lotus.GameModes;

public interface IRoleHolder
{
    public List<CustomRole> MainRoles { get; set; }

    public List<CustomRole> ModifierRoles { get; set; }

    public List<CustomRole> SpecialRoles { get; set; }

    public List<CustomRole> AllRoles { get; set; }
}