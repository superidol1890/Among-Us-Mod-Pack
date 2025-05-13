using Lotus.Roles.Interactions.Interfaces;
using Lotus.Extensions;

namespace Lotus.Roles.Interactions;

public class LotusInteraction : Interaction
{
    public static Stub FatalInteraction = new(new FatalIntent());
    public static Stub HostileInteraction = new(new HostileIntent());
    public static Stub NeutralInteraction = new(new NeutralIntent());
    public static Stub HelpfulInteraction = new(new HelpfulIntent());

    private CustomRole unifiedRoleDefinition;

    public LotusInteraction(Intent intent, CustomRole roleDefinition)
    {
        this.Intent = intent;
        this.unifiedRoleDefinition = roleDefinition;
    }

    public CustomRole Emitter() => unifiedRoleDefinition;
    public Intent Intent { get; set; }
    public bool IsPromised { get; set; }

    public virtual Interaction Modify(Intent intent) => new LotusInteraction(intent, unifiedRoleDefinition);

    public class Stub
    {
        private Intent intent;
        public Stub(Intent intent)
        {
            this.intent = intent;
        }

        public LotusInteraction Create(CustomRole role)
        {
            return new LotusInteraction(intent, role);
        }

        public LotusInteraction Create(PlayerControl player)
        {
            return new LotusInteraction(intent, player.PrimaryRole());
        }
    }
}