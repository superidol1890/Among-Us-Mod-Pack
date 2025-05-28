using System;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Options.UI;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Options.Client;

[Localized(ModConstants.Options)]
public class SoundOptions
{
    public enum SoundTypes
    {
        Off = 0,
        Default
    }

    private static readonly object[] AllSoundTypes = new object[] { (int)SoundTypes.Off, (int)SoundTypes.Default };

    public SoundTypes CurrentSoundType
    {
        get => (SoundTypes)_soundType;
        set
        {
            int index = (int)value;
            lobbyMusicOption.SetValue(index);
            _soundType = index;
        }
    }

    private int _soundType;

    private GameOption lobbyMusicOption;

    public SoundOptions()
    {
        OptionManager optionManager = OptionManager.GetManager(file: "sound.txt", managerFlags: OptionManagerFlags.IgnorePreset);
        lobbyMusicOption = new GameOptionBuilder()
            .Values(AllSoundTypes)
            .Key("Lobby Music Type")
            .Name("Lobby Music")
            .Description("What sound to play in Lobby")
            .IOSettings(s => s.UnknownValueAction = ADEAnswer.Allow)
            .BindInt(i =>
            {
                optionManager.DelaySave(0);
                _soundType = i;
            })
            .BuildAndRegister(optionManager);
    }


}