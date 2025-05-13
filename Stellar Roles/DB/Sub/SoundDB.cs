﻿using System;
using System.Text.Json.Serialization;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.DB;

/// <summary>
///     Database of Among Us AudioClips
/// </summary>
public class SoundDB(SerializedAssetDB serializedDB) : SubDB<AudioClip>(serializedDB)
{
    public override void LoadShip(ShipStatus shipStatus, MapType mapType)
    {
        DB.SoundDB.ForEach(elem =>
        {
            if (elem.MapType != mapType)
                return;

            // Transform
            var transform = shipStatus.transform.Find(elem.Path);
            if (transform == null)
            {
                LILogger.Warn($"SoundDB could not find {elem.ID} in {shipStatus.name}");
                return;
            }

            // Components
            var shipRoom = transform.gameObject.GetComponent<SkeldShipRoom>();
            var stepWatcher = transform.gameObject.GetComponent<FootstepWatcher>();
            AudioClip? audioClip = null;
            if (shipRoom != null)
            {
                audioClip = SearchSoundGroup(shipRoom.FootStepSounds, elem.Name);
            }
            else if (stepWatcher != null)
            {
                audioClip = SearchSoundGroup(stepWatcher.Sounds, elem.Name);
            }
            else
            {
                LILogger.Warn($"SoundDB could not find component for {elem.ID}");
                return;
            }

            // AudioClip
            if (audioClip == null)
            {
                LILogger.Warn($"SoundDB could not find AudioClip for {elem.ID}");
                return;
            }

            Add(elem.ID, audioClip);
        });
    }

    private AudioClip? SearchSoundGroup(SoundGroup? soundGroup, string name)
    {
        if (soundGroup == null)
            return null;
        foreach (var clip in soundGroup.Clips)
            if (clip.name == name)
                return clip;

        return null;
    }

    [Serializable]
    public class DBElement
    {
        public string ID { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public int Map { get; set; }

        [JsonIgnore] public MapType MapType => (MapType)Map;
    }
}