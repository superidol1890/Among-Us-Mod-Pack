using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Properties;

public class RoleProperties : IEnumerable<RoleProperty>
{
    public static NamespacedKey<RoleProperties> Key = NamespacedKey.Lotus<RoleProperties>(nameof(RoleProperties));
    private HashSet<RoleProperty> Properties { get; } = new();

    public int Count => Properties.Count;

    public void Add(RoleProperty roleProperty) => Properties.Add(roleProperty);

    public void AddAll(IEnumerable<RoleProperty> roleProperty) => roleProperty.ForEach(rP => Properties.Add(rP));

    public void AddAll(params RoleProperty[] properties) => properties.ForEach(rP => Properties.Add(rP));

    public Func<RoleProperties, RoleProperties> ConcatFunction() => props =>
    {
        this.AddAll(props);
        return this;
    };

    public void Remove(RoleProperty roleProperty) => Properties.Remove(roleProperty);

    public bool HasProperty(RoleProperty roleProperty) => Properties.Contains(roleProperty);

    public bool HasProperty(string name) => Properties.Any(p => p.Name == name);

    public IEnumerator<RoleProperty> GetEnumerator() => Properties.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    // Useful Util Methods

    public static bool IsModifier(CustomRole roleDefinition) => HasProperty(roleDefinition, RoleProperty.IsModifier);
    public static bool IsAbleToKill(CustomRole roleDefinition) => HasProperty(roleDefinition, RoleProperty.IsAbleToKill);
    public static bool CannotWinAlone(CustomRole roleDefinition) => HasProperty(roleDefinition, RoleProperty.CannotWinAlone);
    public static bool IsApparition(CustomRole roleDefinition) => HasProperty(roleDefinition, RoleProperty.IsApparition);

    public static bool HasProperty(CustomRole roleDefinition, RoleProperty property) => GetProperties(roleDefinition).Compare(rp => rp.HasProperty(property));
    public static bool IsSpecialType(CustomRole roleDefinition, SpecialType specialType) => roleDefinition.Metadata.GetOrEmpty(LotusKeys.AuxiliaryRoleType).Compare(rp => rp == specialType);


    public static Optional<RoleProperties> GetProperties(CustomRole definition) => definition.Metadata.GetOrEmpty(Key);
}