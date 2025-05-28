using Lotus.Factions.Interfaces;
using Lotus.Options;
using UnityEngine;

namespace Lotus.Factions.Neutrals;
public class Neutral : Faction<Neutral>, INeutralFaction<Neutral>
{
    private readonly string factionName;

    public Neutral(string? factionName = null)
    {
        this.factionName = factionName ?? FactionTranslations.Neutral.Name;
    }

    public override string Name() => this.factionName;

    public override Relation Relationship(Neutral sameFaction) => Relation.None;

    public override bool CanSeeRole(PlayerControl player) => RoleOptions.LoadNeutralOptions().NeutralsKnowAlliedRoles;

    public override Color Color => Color.gray;

    public override Relation RelationshipOther(IFaction other) => Relation.None;
}