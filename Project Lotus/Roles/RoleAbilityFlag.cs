using System;

namespace Lotus.Roles;

[Flags]
public enum RoleAbilityFlag
{
    CannotVent = 2, // player cannot vent
    CannotSabotage = 4, // player cannot sabotage (excluding doors since we don't know who sent the rpc.)
    UsesPet = 8, // role uses the pet button (not used rn, but may be used later if innersloth doesnt like pet roles)
    IsAbleToKill = 16, // role is able to kill
}


