using Lotus.Factions;
using Lotus.Factions.Crew;
using Lotus.Factions.Impostors;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Neutrals;
using Lotus.Factions.Undead;
using Lotus.Roles.RoleGroups.NeutralKilling;
using UnityEngine;

namespace Lotus.Roles.Factions;

public class JackalFaction: Faction<JackalFaction>, INeutralFaction<JackalFaction>
{
    public static JackalFaction Instance { get; } = new();

    public override string Name() => FactionTranslations.NeutralKillers.Name;

    public override Relation Relationship(JackalFaction sameFaction) => Relation.FullAllies;

    public override bool CanSeeRole(PlayerControl player) => true;

    public override Color Color => Jackal.JackalColor;

    public override Relation RelationshipOther(IFaction other)
    {
        return other switch
        {
            TheUndead => Relation.None,
            Crewmates => Relation.None,
            ImpostorFaction => Relation.None,
            Neutral when other.GetType() == typeof(Neutral) => Relation.None,
            _ => other.Relationship(this)
        };
    }
}