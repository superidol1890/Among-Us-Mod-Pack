using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace MiraAPI.GameOptions;

/// <summary>
/// Base class for option groups. An option group is a collection of options that are displayed together in the options menu.
/// </summary>
public abstract class AbstractOptionGroup
{
    internal List<IModdedOption> Options { get; } = [];

    /// <summary>
    /// Gets a list of the options which are a part of this group.
    /// </summary>
    public ReadOnlyCollection<IModdedOption> Children => new(Options);

    /// <summary>
    /// Gets the name of the group. Visible in options menu.
    /// </summary>
    public abstract string GroupName { get; }

    /// <summary>
    /// Gets the Optionable type of the group.
    /// </summary>
    public virtual Type? OptionableType => null;

    /// <summary>
    /// Gets a value indicating whether the group should be shown in the modifiers menu.
    /// </summary>
    // TODO: make this not a boolean
    public virtual bool ShowInModifiersMenu => false;

    /// <summary>
    /// Gets the function that determines whether the group should be visible or not.
    /// </summary>
    public virtual Func<bool> GroupVisible => () => true;

    /// <summary>
    /// Gets the group color. This is used to color the group in the options menu.
    /// </summary>
    public virtual Color GroupColor => MiraApiPlugin.DefaultHeaderColor;

    /// <summary>
    /// Gets the group priority. This is used to determine the order in which groups are displayed in the options menu.
    /// Zero is the highest priority, and the default value is the max uint value.
    /// </summary>
    public virtual uint GroupPriority => uint.MaxValue;

    internal bool AllOptionsHidden { get; set; }

    internal CategoryHeaderMasked? Header { get; set; }
}

/// <summary>
/// Base class for option groups. An option group is a collection of options that are displayed together in the options menu.
/// </summary>
/// <typeparam name="T">The type of the optionable that this group contains.</typeparam>
public abstract class AbstractOptionGroup<T> : AbstractOptionGroup where T : IOptionable
{
    /// <inheritdoc />
    public override Type OptionableType => typeof(T);
}
