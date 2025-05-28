using Lotus.Factions.Interfaces;

namespace Lotus.Factions.Neutrals;

/// <summary>
/// Marks a class as a neutral faction
/// </summary>
public interface INeutralFaction<in T> : INeutralFaction where T : INeutralFaction<T>
{

}

public interface INeutralFaction
{

}