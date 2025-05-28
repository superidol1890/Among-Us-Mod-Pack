using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Roles;
using Lotus.Roles.GUI;
using UnityEngine;
using VentLib.Utilities.Extensions;

namespace Lotus.GUI.Patches;

[HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.SetFromSettings))]
public class SetAbilitySettingsPatch
{
    public static bool Prefix(AbilityButton __instance)
    {
        List<CustomRole> customRoles = PlayerControl.LocalPlayer.GetAllRoleDefinitions().ToList();
        customRoles.Do(cr => cr.UIManager.ForceUpdate());
        bool anyOverride = customRoles.Any(cr =>
        {
            RoleUIManager provider = cr.UIManager;
            return provider is { Initialized: true, AbilityButton.IsOverriding: true };
        });
        return !anyOverride;
    }
}