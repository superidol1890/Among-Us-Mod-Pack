using Lotus.Roles.RoleGroups.Crew.Ingredients;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Crew.Potions;

public class PotionProtection : Potion
{
    [Localized("Protection")]
    public static string PotionName = "Castling Brew";


    public PotionProtection(int requiredCatalyst) : base((2, Ingredient.Catalyst), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override string Name() => PotionName;

    public override Color Color() => new(1f, 1f, 0.4f);

    public override bool Use(PlayerControl user)
    {
        Alchemist alchemist = user.PrimaryRole<Alchemist>();
        alchemist.IsProtected = true;
        Async.Schedule(() => alchemist.IsProtected = false, 80f);
        return true;
    }
}