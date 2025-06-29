using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using MiraAPI.Events.Mira;
using MiraAPI.PluginLoading;

namespace MiraAPI.Hud;

/// <summary>
/// Custom button manager for handling custom buttons.
/// </summary>
public static class CustomButtonManager
{
    /// <summary>
    /// Gets a list of all registered custom buttons.
    /// </summary>
    public static ReadOnlyCollection<CustomActionButton> Buttons { get; internal set; } = new([]);

    internal static readonly List<CustomActionButton> CustomButtons = [];
    internal static readonly Dictionary<Type, Type> ButtonEventTypes = [];
    internal static readonly Dictionary<Type, Type> ButtonCancelledEventTypes = [];

    internal static bool RegisterButton(Type buttonType, MiraPluginInfo pluginInfo)
    {
        if (!buttonType.IsAssignableTo(typeof(CustomActionButton)) || Activator.CreateInstance(buttonType) is not CustomActionButton button)
        {
            return false;
        }

        CustomButtons.Add(button);
        pluginInfo.InternalButtons.Add(button);
        typeof(CustomButtonSingleton<>).MakeGenericType(buttonType)
#pragma warning disable S3011
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
#pragma warning restore S3011
            .SetValue(null, button);

        ButtonEventTypes.Add(buttonType, typeof(MiraButtonClickEvent<>).MakeGenericType(buttonType));
        ButtonCancelledEventTypes.Add(buttonType, typeof(MiraButtonCancelledEvent<>).MakeGenericType(buttonType));

        return true;
    }
}
