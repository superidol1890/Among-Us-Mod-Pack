using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.Roles;

namespace Lotus.GameModes;
public abstract class RoleHolder : IRoleHolder
{
    public abstract List<Action> FinishedCallbacks();

    public List<CustomRole> MainRoles { get; set; }

    public List<CustomRole> ModifierRoles { get; set; }

    public List<CustomRole> SpecialRoles { get; set; }

    public List<CustomRole> AllRoles { get; set; }

    public bool Intialized
    {
        get { return _initialized; }
        set
        {
            if (value == true)
                FinishedCallbacks().ForEach(finc => finc());

            _initialized = value;
        }
    }
    private bool _initialized;
}