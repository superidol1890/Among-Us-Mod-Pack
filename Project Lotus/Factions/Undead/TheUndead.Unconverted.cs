using Lotus.Factions.Interfaces;
using Lotus.GUI.Name.Components;
using VentLib.Utilities.Collections;

namespace Lotus.Factions.Undead;

public partial class TheUndead
{
    public class Unconverted : TheUndead, ISubFaction<TheUndead>
    {
        public IFaction PreviousFaction { get; }
        public Remote<IndicatorComponent> Indicator { get; }

        public Unconverted(IFaction previousFaction, Remote<IndicatorComponent> indicator)
        {
            this.PreviousFaction = previousFaction;
            this.Indicator = indicator;
        }

        public Relation MainFactionRelationship() => Relation.SharedWinners;

        public Relation Relationship(ISubFaction<TheUndead> subFaction)
        {
            return subFaction switch
            {
                Origin => Relation.SharedWinners,
                Converted => Relation.SharedWinners,
                Unconverted => Relation.SharedWinners,
                _ => subFaction.Relationship(this)
            };
        }

        public override bool CanSeeRole(PlayerControl player) => false;
    }
}