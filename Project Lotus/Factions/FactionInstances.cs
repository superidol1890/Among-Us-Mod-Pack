using System;
using System.Collections.Generic;
using Lotus.Factions.Crew;
using Lotus.Factions.Impostors;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Neutrals;
using Lotus.Factions.Undead;
using VentLib.Utilities;

namespace Lotus.Factions;

public class FactionInstances
{
    public static Crewmates Crewmates { get; } = new();
    public static ImpostorFaction Impostors { get; } = new();
    public static Madmates Madmates { get; } = new();
    public static TheUndead TheUndead { get; } = new TheUndead.Origin();
    public static Neutral Neutral { get; } = new();
    public static Modifiers Modifiers { get; } = new();

    internal static Dictionary<Type, IFaction> AddonFactions = new();
    public static IFaction GetExternalFaction(Type factionType)
    {
        if (AddonFactions.TryGetValue(factionType, out IFaction? faction)) return faction;
        else
        {
            StaticLogger.Fatal($"Could not find {factionType.FullName} in AddonFactions. Caller: {Mirror.GetCaller()?.Name}");
            return null!;
        }
    }
}