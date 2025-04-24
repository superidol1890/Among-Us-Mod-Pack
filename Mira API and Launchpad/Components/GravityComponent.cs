using LaunchpadReloaded.Modifiers;
using Reactor.Utilities.Attributes;
using System;
using MiraAPI.Modifiers;
using UnityEngine;

namespace LaunchpadReloaded.Components;

[RegisterInIl2Cpp]
public class GravityComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl? gravityGuy;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        var player = collider.gameObject.GetComponent<PlayerControl>();
        if (player == null || player == gravityGuy)
        {
            return;
        }

        if (!player.HasModifier<GravityVictimModifier>())
        {
            player.GetModifierComponent().AddModifier(new GravityVictimModifier(gravityGuy!));
        }
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        var player = collider.gameObject.GetComponent<PlayerControl>();
        if (player == null)
        {
            return;
        }

        if (player.HasModifier<GravityVictimModifier>())
        {
            player.GetModifierComponent().RemoveModifier<GravityVictimModifier>();
        }
    }
}