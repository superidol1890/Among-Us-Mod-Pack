using Lotus.Factions.Crew;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Neutrals;
using Lotus.Factions.Undead;
using UnityEngine;

namespace Lotus.Factions.Impostors;

public class ImpostorFaction : Faction<ImpostorFaction>
{
    public override string Name() => FactionTranslations.Impostors.Name;

    public override Relation Relationship(ImpostorFaction sameFaction) => Relation.FullAllies;

    public override bool CanSeeRole(PlayerControl player) => true;

    public override Color Color => Color.red;

    public override Relation RelationshipOther(IFaction other)
    {
        return other switch
        {
            TheUndead => Relation.None,
            Crewmates => Relation.None,
            Neutral when other.GetType() == typeof(Neutral) => Relation.None,
            _ => other.Relationship(this)
        };
    }
}