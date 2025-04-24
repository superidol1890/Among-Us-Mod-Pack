using MiraAPI.Modifiers;
using Reactor.Utilities.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaunchpadReloaded.Components;

[RegisterInIl2Cpp]
public class ModifierStorageComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    public List<BaseModifier>? Modifiers;
}