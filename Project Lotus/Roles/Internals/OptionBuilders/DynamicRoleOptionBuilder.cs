﻿extern alias JBAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using JBAnnotations::JetBrains.Annotations;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.Internals.OptionBuilders;

[UsedImplicitly]
public class DynamicRoleOptionBuilder
{
    private Dictionary<Type, bool> roleValues = new();
    private readonly Dictionary<DynamicOptionPredicate, EnabledType> enabledTypes = new();
    private List<DynamicOptionPredicate>? predicates;

    private Func<CustomRole, bool> defaultPredicate;
    public DynamicRoleOptionBuilder(List<DynamicOptionPredicate>? predicates = null, Func<CustomRole, bool>? defaultPredicate = null)
    {
        this.predicates = predicates;
        this.defaultPredicate = defaultPredicate ?? (_ => false);
    }

    public static DynamicRoleOptionBuilder Standard(string neutralKillingName, string neutralPassiveName, string madmateName, Func<CustomRole, bool>? defaultPredicate = null)
    {
        return new DynamicRoleOptionBuilder(new List<DynamicOptionPredicate>
        {
            new(r => r.SpecialType is SpecialType.NeutralKilling, "Neutral Killing Settings", neutralKillingName, false),
            new(r => r.SpecialType is SpecialType.Neutral, "Neutral Passive Settings", neutralPassiveName, false),
            new(r => r.Faction is Lotus.Factions.Impostors.Madmates, "Madmates Settings", madmateName, false)
        }, defaultPredicate);
    }

    public bool IsAllowed(CustomRole role)
    {
        Type roleType = role.GetType();
        if (!roleValues.TryGetValue(roleType, out bool allowed)) return defaultPredicate(role);
        EnabledType enabledType = enabledTypes.Where(et => et.Key.Predicate(role)).Select(et => et.Value).FirstOrDefault();
        return enabledType switch
        {
            EnabledType.Custom => allowed,
            EnabledType.Off => false,
            EnabledType.On => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    public GameOptionBuilder Decorate(GameOptionBuilder builder, List<DynamicOptionPredicate>? optionPredicates = null, string? onText = null, string? offText = null)
    {
        predicates ??= optionPredicates;
        if (predicates == null) throw new ArgumentNullException(nameof(optionPredicates));

        onText ??= GeneralOptionTranslations.OnText;
        offText ??= GeneralOptionTranslations.OffText;

        Dictionary<DynamicOptionPredicate, GameOptionBuilder> builders = new();
        ProjectLotus.GameModeManager.CurrentGameMode.RoleManager.AllCustomRoles().OrderBy(r => r.RoleName).ForEach(r =>
        {
            Type roleType = r.GetType();
            predicates.FirstOrOptional(p => p.Predicate(r)).IfPresent(np =>
            {
                GameOptionBuilder b = builders.GetOrCompute(np, () =>
                {
                    return new GameOptionBuilder()
                        .Name(np.Name)
                        .Key(np.Key)
                        .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(1).Color(Color.red).Build())
                        .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(2).Color(Color.green).Build())
                        .Value(v => v.Text(GeneralOptionTranslations.CustomText).Value(0).Color(new Color(0.73f, 0.58f, 1f)).Build())
                        .ShowSubOptionPredicate(i => (int)i == 0)
                        .BindInt(i => enabledTypes[np] = (EnabledType)i);
                });
                b.SubOption(sub => sub.KeyName(r.RoleName, r.RoleColor.Colorize(r.RoleName))
                    .AddEnableDisabledValues(np.DefaultOn, onText, offText)
                    .BindBool(bb => roleValues[roleType] = bb)
                    .Build());
            });
        });
        builders.Values.ForEach(b =>
        {
            builder.SubOption(_ => b.Build());
        });
        return builder;
    }
}

public class DynamicOptionPredicate
{
    public Func<CustomRole, bool> Predicate;
    public string Key;
    public string Name;
    public bool DefaultOn;

    public DynamicOptionPredicate(Func<CustomRole, bool> predicate, string key, string name, bool defaultOn = true)
    {
        Predicate = predicate;
        Key = key;
        Name = name;
        DefaultOn = defaultOn;
    }
}

public enum EnabledType
{
    Custom,
    Off,
    On
}