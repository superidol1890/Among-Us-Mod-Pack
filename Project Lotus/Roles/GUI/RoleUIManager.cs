using System.Collections.Generic;
using HarmonyLib;
using Lotus.Logging;
using Lotus.Roles.Attributes;
using Lotus.Roles.GUI.Interfaces;
using Lotus.Roles.Interfaces;

namespace Lotus.Roles.GUI;

[InstantiateOnSetup(true, true)]
public class RoleUIManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(RoleUIManager));
    internal bool Initialized = false;

    public AbstractBaseRole BaseRole { get; private set; } = null!;

    public RoleButton PetButton = null!;
    public RoleButton UseButton = null!;
    public RoleButton ReportButton = null!;
    public RoleButton VentButton = null!;
    public RoleButton KillButton = null!;
    public RoleButton AbilityButton = null!;

    [SetupInjected] private List<RoleButton> modifiedButtons = [];
    private bool enabled = true;

    public void Start(PlayerControl _)
    {
        if (BaseRole is not IRoleUI roleUI || !enabled) return;
        try
        {
            BaseRole.MyPlayer.Data.Role.InitializeAbilityButton();
            InitializeButtons();
            HandleUIComponent(roleUI);
            modifiedButtons.ForEach(b => b.CompleteLoad());
        }
        catch (System.Exception ex)
        {
            log.Exception(ex);
        }
    }

    public void Update()
    {
        modifiedButtons.Do(b => b.Update());
        // updateComponents.ForEach(c => c.UpdateGUI(this)); Use FixedUpdate lol. this useless.
    }
    internal void ForceUpdate()
    {
        modifiedButtons.ForEach(b => b.ForceUpdate());
    }

    public virtual void HandleUIComponent(IRoleUI component)
    {
        log.Debug($"Handling GUI component: {this.GetHashCode()} | {component.GetHashCode()}");

        if (PetButton != null!)
        {
            PetButton = component.PetButton(PetButton);
            if (PetButton.IsOverriding) RegisterModification(PetButton);
        }

        if (UseButton != null!)
        {
            UseButton = component.UseButton(UseButton);
            if (UseButton.IsOverriding) RegisterModification(UseButton);
        }

        if (VentButton != null!)
        {
            VentButton = component.VentButton(VentButton);
            if (VentButton.IsOverriding) RegisterModification(VentButton);
        }

        if (ReportButton != null!)
        {
            ReportButton = component.ReportButton(ReportButton);
            if (ReportButton.IsOverriding) RegisterModification(ReportButton);
        }

        if (KillButton != null!)
        {
            KillButton = component.KillButton(KillButton);
            if (KillButton.IsOverriding) RegisterModification(KillButton);
        }

        if (AbilityButton != null!)
        {
            AbilityButton = component.AbilityButton(AbilityButton);
            if (AbilityButton.IsOverriding) RegisterModification(AbilityButton);
        }
    }

    public virtual void InitializeButtons()
    {
        PetButton = new RoleButton(() => AmongUsButtonSpriteReferences.PetButton);
        UseButton = new RoleButton( () => AmongUsButtonSpriteReferences.UseButton);
        ReportButton = new RoleButton(() => AmongUsButtonSpriteReferences.ReportButton);
        VentButton = new RoleButton(() => AmongUsButtonSpriteReferences.VentButton);
        AbilityButton = new RoleButton(() => AmongUsButtonSpriteReferences.AbilityButton);
        AbilityButton.GetButton().SetInfiniteUses();
        KillButton = new RoleButton(() => AmongUsButtonSpriteReferences.KillButton);
        Initialized = true;
    }

    public void SetBaseRole(AbstractBaseRole definition) => BaseRole = definition;
    public void DisableUI() => enabled = false;
    public void EnableUI() => enabled = true;

    protected RoleButton RegisterModification(RoleButton button)
    {
        modifiedButtons.Add(button);
        return button;
    }

}