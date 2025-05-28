namespace Lotus.Roles.GUI.Interfaces;

public interface IRoleUI
{
    public RoleButton PetButton(IRoleButtonEditor petButton) => petButton.Default(true);

    public RoleButton UseButton(IRoleButtonEditor useButton) => useButton.Default(true);

    public RoleButton ReportButton(IRoleButtonEditor reportButton) => reportButton.Default(true);

    public RoleButton VentButton(IRoleButtonEditor ventButton) => ventButton.Default(true);

    public RoleButton KillButton(IRoleButtonEditor killButton) => killButton.Default(true);

    public RoleButton AbilityButton(IRoleButtonEditor abilityButton) => abilityButton.Default(true);
}