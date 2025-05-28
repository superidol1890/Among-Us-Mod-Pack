using Lotus.Roles.RoleGroups.Crew.Ingredients;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Crew.Potions;

public class PotionSabotage : Potion
{
    [Localized("Sabotage")]
    public static string PotionName = "Mechanic's Mix";

    public PotionSabotage(int requiredCatalyst) : base((1, Ingredient.Tinkering), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override string Name() => PotionName;

    public override Color Color() => UnityEngine.Color.blue;

    public override bool Use(PlayerControl user)
    {
        user.PrimaryRole<Alchemist>().QuickFixSabotage = true;
        return true;
    }
}