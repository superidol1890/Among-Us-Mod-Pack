using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Interfaces;
using UnityEngine;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.GUI.Name.Holders;

public class ComponentHolder<T> : RemoteList<T>, IComponentHolder<T> where T : INameModelComponent
{
    protected DisplayStyle DisplayStyle;
    protected float Size = 2.925f;
    protected int DisplayLine;
    protected int Spacing = 0;

    protected readonly Dictionary<byte, bool> updated = new();
    protected readonly Dictionary<byte, string> CacheStates = new();
    private readonly List<Action<INameModelComponent>> eventConsumers = new();

    public ComponentHolder(int line = 0, DisplayStyle displayStyle = DisplayStyle.All)
    {
        this.DisplayLine = line;
        this.DisplayStyle = displayStyle;
    }

    public RemoteList<T> Components() => this;

    public void SetSize(float size) => Size = size;

    public void SetLine(int line) => DisplayLine = line;

    public int Line() => DisplayLine;

    public void SetSpacing(int spacing) => this.Spacing = spacing;

    public virtual string Render(PlayerControl player, GameState state)
    {
        List<string> endString = new();
        ViewMode lastMode = ViewMode.Absolute;
        IEnumerable<T> allComponents;
        switch (this.DisplayStyle)
        {
            case DisplayStyle.FirstOnly:
                allComponents = this.Where(p => p.GameStates().Contains(state)).Where(p => p.Viewers().Any(pp => pp.PlayerId == player.PlayerId)).Where((_, i) => i == 0);
                break;
            case DisplayStyle.LastOnly:
                allComponents = this.Where(p => p.GameStates().Contains(state)).Where(p => p.Viewers().Any(pp => pp.PlayerId == player.PlayerId));
                int count = allComponents.Count() - 1;
                allComponents = allComponents.Where((_, i) => i == count);
                break;
            case DisplayStyle.All:
            default:
                allComponents = this.Where(p => p.GameStates().Contains(state)).Where(p => p.Viewers().Any(pp => pp.PlayerId == player.PlayerId));
                break;
        }
        foreach (T component in allComponents)
        {
            ViewMode newMode = component.ViewMode();
            if (newMode is ViewMode.Replace or ViewMode.Absolute || lastMode is ViewMode.Overriden) endString.Clear();
            lastMode = newMode;
            string text = component.GenerateText(state);
            if (text == null) continue;
            endString.Add(text);
            if (newMode is ViewMode.Absolute) break;
        }

        string newString = endString.Join(delimiter: " ".Repeat(Spacing - 1));

        updated[player.PlayerId] = CacheStates.GetValueOrDefault(player.PlayerId, "") != newString;
        return CacheStates[player.PlayerId] = newString;
    }

    public bool Updated(byte playerId) => updated.GetValueOrDefault(playerId, false);

    public new virtual Remote<T> Add(T component)
    {
        Remote<T> remote = base.Add(component);
        eventConsumers.ForEach(ev => ev(component));
        return remote;
    }

    public void AddListener(Action<INameModelComponent> eventConsumer) => eventConsumers.Add(eventConsumer);
}

public enum DisplayStyle
{
    All,
    FirstOnly,
    LastOnly
}