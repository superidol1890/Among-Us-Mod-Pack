using Reactor.Utilities.Attributes;
using System;
using UnityEngine;

namespace LaunchpadReloaded.Components;

[RegisterInIl2Cpp]
public class SealedVentComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl? Sealer;
}