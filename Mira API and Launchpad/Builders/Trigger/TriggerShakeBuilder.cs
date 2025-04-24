﻿using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class TriggerShakeBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-triggershake")
            return;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Trigger Area
        var shakeArea = obj.AddComponent<LIShakeArea>();
        shakeArea.SetParameters(
            elem.properties.shakeAmount ?? 0.03f,
            elem.properties.shakePeriod ?? 400.0f
        );
    }
}