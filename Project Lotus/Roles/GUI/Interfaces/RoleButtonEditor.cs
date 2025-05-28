using Lotus.GUI;
using System;
using UnityEngine;

namespace Lotus.Roles.GUI.Interfaces;

public interface IRoleButtonEditor
{
    public RoleButton BindCooldown(Cooldown? cooldown);
    public RoleButton BindUses(Func<int>? usesSupplier);
    public RoleButton SetSprite(Func<Sprite> spriteLoadingFunction);
    public RoleButton SetText(string localizedText);
    public RoleButton SetMaterial(Material material);
    public RoleButton Default(bool skipThisButton);
}