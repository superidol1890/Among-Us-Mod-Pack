using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.Roles;
using Lotus.Roles.RoleGroups.Colorwars;

namespace Lotus.GameModes.Colorwars;

public class ColorwarsRoles : RoleHolder
{
    public override List<Action> FinishedCallbacks() => Callbacks;

    public static List<Action> Callbacks { get; set; } = new List<Action>();

    public static ColorwarsRoles Instance = null!;

    public StaticRoles Static;

    public ColorwarsRoles()
    {
        Instance = this;

        AllRoles = new List<CustomRole>();
        Static = new StaticRoles();

        AllRoles.AddRange(Static.GetType()
            .GetFields()
            .Select(f => (CustomRole)f.GetValue(Static)!)
            .ToList());


        // solidify every role to finish them off
        AllRoles.ForEach(r => r.Solidify());
    }

    public class StaticRoles
    {
        public Painter Painter = new();
    }
}