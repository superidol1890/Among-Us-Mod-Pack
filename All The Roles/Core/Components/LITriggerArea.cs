﻿using System;
using LevelImposter.Trigger;

namespace LevelImposter.Core;

/// <summary>
///     Object that fires a trigger when the player enters/exits it's range
/// </summary>
public class LITriggerArea(IntPtr intPtr) : PlayerArea(intPtr)
{
    private const string ENTER_TRIGGER_ID = "onEnter";
    private const string EXIT_TRIGGER_ID = "onExit";

    private bool _isClientSide;

    /// <summary>
    ///     Sets whether or not the Trigger Area is client sided
    /// </summary>
    /// <param name="isClientSide">TRUE if the trigger is client sided</param>
    public void SetClientSide(bool isClientSide)
    {
        _isClientSide = isClientSide;
    }

    protected override void OnPlayerEnter(PlayerControl player)
    {
        var triggerServerSided = CurrentPlayersIDs?.Count <= 1 && !_isClientSide;
        var triggerClientSided = player.AmOwner && _isClientSide;
        if (triggerClientSided || triggerServerSided)
        {
            TriggerSignal signal = new(gameObject, ENTER_TRIGGER_ID, player);
            TriggerSystem.GetInstance().FireTrigger(signal);
        }
    }

    protected override void OnPlayerExit(PlayerControl player)
    {
        var triggerServerSided = CurrentPlayersIDs?.Count <= 0 && !_isClientSide;
        var triggerClientSided = player.AmOwner && _isClientSide;
        if (triggerClientSided || triggerServerSided)
        {
            TriggerSignal signal = new(gameObject, EXIT_TRIGGER_ID, player);
            TriggerSystem.GetInstance().FireTrigger(signal);
        }
    }
}