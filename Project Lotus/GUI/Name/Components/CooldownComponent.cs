using System;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Impl;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.GUI.Name.Components;

public class CooldownComponent : SimpleComponent
{
    private Cooldown? cooldown;
    private Func<Cooldown>? cooldownSupplier;
    private Color numberColor = Color.white;
    private Color textColor = new(0.93f, 0.57f, 0.28f);
    private Ubifix prefix;

    public CooldownComponent(Cooldown cooldown, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, string cdText = "CD: ", params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.cooldown = cooldown;
        prefix = new Ubifix(new LiveString(() => this.cooldown.NotReady() ? textColor.Colorize(cdText) : ""));
        this.AddPrefix(prefix);
        this.SetMainText(new LiveString(TextSupplier));
    }

    public CooldownComponent(Func<Cooldown> cooldownSupplier, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, string cdText = "CD: ", params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.cooldownSupplier = cooldownSupplier;
        prefix = new Ubifix(new LiveString(() => Cooldown.NotReady() ? textColor.Colorize(cdText) : ""));
        this.AddPrefix(prefix);
        this.SetMainText(new LiveString(TextSupplier));
    }

    public CooldownComponent(Func<Cooldown> cooldownSupplier, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive, string cdText = "CD: ", params PlayerControl[] viewers) : this(cooldownSupplier, [gameState], viewMode, cdText, viewers)
    {
    }

    public CooldownComponent(Cooldown cooldown, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive, string cdText = "CD: ", params PlayerControl[] viewers) : this(cooldown, [gameState], viewMode, cdText, viewers)
    {
    }

    public CooldownComponent SetPrefix(string prefix)
    {
        this.prefix.Delete();
        this.prefix = new Ubifix(new LiveString(() => Cooldown.NotReady() ? textColor.Colorize(prefix) : ""));
        this.AddPrefix(this.prefix);
        return this;
    }

    public CooldownComponent SetNumberColor(Color color)
    {
        this.numberColor = color;
        return this;
    }

    public CooldownComponent SetTextColor(Color color)
    {
        this.textColor = color;
        return this;
    }

    private string TextSupplier()
    {
        string cooldownString = Cooldown.IsReady() ? "" : (Cooldown + "s");
        return numberColor.Colorize(cooldownString);
    }

    public Cooldown Cooldown => cooldown ?? cooldownSupplier!();
}