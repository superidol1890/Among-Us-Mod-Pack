using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.API;
using Lotus.API.Player;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class PlayerSavedEvent : IRecipientEvent
{
    private FrozenPlayer? savedPlayer;
    private Optional<CustomRole> playerRole;
    private Optional<FrozenPlayer> savior;
    private Optional<CustomRole> saviorRole;
    private Optional<PlayerControl> killer;
    private Optional<CustomRole> killerRole;

    private Timestamp timestamp = new();

    public PlayerSavedEvent(PlayerControl savedPlayer, PlayerControl? savior, PlayerControl? killer)
    {
        this.savedPlayer = Game.MatchData.GetFrozenPlayer(savedPlayer);
        playerRole = Optional<CustomRole>.Of(savedPlayer.PrimaryRole());
        this.savior = Optional<FrozenPlayer>.Of(Game.MatchData.GetFrozenPlayer(savior));
        this.saviorRole = this.savior.FlatMap(p => new UnityOptional<PlayerControl>(p.MyPlayer)).Map(p => p.PrimaryRole());
        this.killer = Optional<PlayerControl>.Of(killer);
        this.killerRole = this.killer.Map(p => p.PrimaryRole());
    }

    public FrozenPlayer Player() => savedPlayer!;

    public Optional<CustomRole> RelatedRole() => playerRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public string Message()
    {
        string killerString = killer.Transform(k => $" from {Game.GetName(k)}", () => "");
        return savior.Transform(player => $"{player.Name} saved {Game.GetName(savedPlayer)}{killerString}.",
            () => $"{Game.GetName(savedPlayer)} was saved{killerString}.");
    }

    public Optional<FrozenPlayer> Instigator() => savior;

    public Optional<CustomRole> InstigatorRole() => saviorRole;

    public Optional<PlayerControl> Killer() => killer;

    public Optional<CustomRole> KillerRole => killerRole;
}