using Lotus.Factions.Interfaces;
using VentLib.Localization.Attributes;

namespace Lotus.Factions.Impostors;

public class Madmates : ImpostorFaction, ISubFaction<ImpostorFaction>
{
    public override string Name() => FactionTranslations.Madmates.Name;

    public Relation MainFactionRelationship() => Relation.SharedWinners;

    public Relation Relationship(ISubFaction<ImpostorFaction> subFaction)
    {
        return subFaction is Madmates ? Relation.SharedWinners : subFaction.Relationship(this);
    }
}