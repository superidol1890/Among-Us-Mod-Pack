﻿using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Manages a singular coroutine across a set of GameObjects
/// </summary>
public class GameObjectCoroutineManager
{
    private readonly Dictionary<int, Coroutine> _activeCoroutines = new();

    private int GetObjectID(GameObject gameObject)
    {
        return gameObject.GetInstanceID();
    }

    /// <summary>
    ///     Starts a coroutine on a game object.
    ///     Automatically stops the existing coroutine if one exists.
    /// </summary>
    /// <param name="gameObject">GameObject to run coroutine on</param>
    /// <param name="coroutine">The coroutine to run</param>
    public void Start(GameObject gameObject, IEnumerator coroutine)
    {
        Stop(gameObject);
        var objectID = GetObjectID(gameObject);
        var newCoroutine = LIShipStatus.GetInstance().StartCoroutine(
            CoRunCoroutine(objectID, coroutine).WrapToIl2Cpp()
        );
        _activeCoroutines[objectID] = newCoroutine;
    }

    /// <summary>
    ///     Stops the coroutine on a game object if one exists
    /// </summary>
    /// <param name="gameObject">GameObject to stop coroutines from</param>
    public void Stop(GameObject gameObject)
    {
        var objectID = GetObjectID(gameObject);
        if (_activeCoroutines.ContainsKey(objectID))
        {
            LIShipStatus.GetInstance().StopCoroutine(_activeCoroutines[objectID]);
            _activeCoroutines.Remove(objectID);
        }
    }

    /// <summary>
    ///     Coroutine to remove the coroutine from the active list on completion
    /// </summary>
    private IEnumerator CoRunCoroutine(int objectID, IEnumerator coroutine)
    {
        yield return coroutine;
        _activeCoroutines.Remove(objectID);
    }
}