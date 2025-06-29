using Reactor.Utilities.Attributes;
using System;
using UnityEngine;

namespace LaunchpadReloaded.Features;

[RegisterInIl2Cpp]
public class LaunchpadPlayer(IntPtr ptr) : MonoBehaviour(ptr)
{
    public Transform? knife;

    public PlayerControl playerObject = null!;

    private void Awake()
    {
        playerObject = gameObject.GetComponent<PlayerControl>();
    }

    private void FixedUpdate()
    {
        knife ??= playerObject.gameObject.transform.FindChild("BodyForms/Seeker/KnifeHand");
        if (knife)
        {
            knife.gameObject.SetActive(!playerObject.Data.IsDead && playerObject.CanMove);
        }
    }
}