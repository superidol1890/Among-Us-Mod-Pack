using Lotus.API;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Overrides;
using UnityEngine;

namespace Lotus.Roles.Subroles;

public class Watcher : Subrole, IRoleCandidate
{
    public bool ShouldSkip() => !AUSettings.AnonymousVotes();
    public override string Identifier() => "▲";

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.AnonymousVoting, false)
            .RoleColor(new Color(0.38f, 0.51f, 0.61f));
}