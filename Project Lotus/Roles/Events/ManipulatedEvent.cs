using Lotus.API.Odyssey;
using Lotus.Managers.History.Events;
using Lotus.API;
using VentLib.Utilities.Optionals;
using Lotus.Roles.RoleGroups.Impostors;

namespace Lotus.Roles.Events;

public class ManipulatedEvent : TargetedAbilityEvent, IRoleEvent
{
    public ManipulatedEvent(PlayerControl killer, PlayerControl victim) : base(killer, victim)
    {
    }

    public override string Message()
    {
        return $"{Game.GetName(Player())} tricked {Game.GetName(Target())}'s mind.";
    }
}

public class ManipulatedPlayerKillEvent : KillEvent
{
    private Optional<PlayerControl> manipulator;

    public ManipulatedPlayerKillEvent(PlayerControl killer, PlayerControl victim, PlayerControl? manipulator, bool success) : base(killer, victim, success)
    {
        this.manipulator = Optional<PlayerControl>.Of(manipulator);
    }

    public override string Message()
    {
        return manipulator.Transform(m => $"${Game.GetName(m)} manipulated {Game.GetName(Player())} to attack {Game.GetName(Target())}.",
            () => $"{Game.GetName(Player())} was controlled to attack {Game.GetName(Target())}.");
    }

    public Optional<PlayerControl> Manipulator() => manipulator;
}

public class ManipulatedPlayerDeathEvent : DeathEvent
{
    public ManipulatedPlayerDeathEvent(PlayerControl victim, PlayerControl killer) : base(victim, killer)
    {
    }

    public override string SimpleName() => Mastermind.MastermindTranslations.ManipulatedText;
}