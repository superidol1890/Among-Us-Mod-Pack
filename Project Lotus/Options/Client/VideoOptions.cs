using System;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Options.UI;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using Lotus.Extensions;

namespace Lotus.Options.Client;

[Localized(ModConstants.Options)]
public class VideoOptions
{
    public static object[] FpsLimits = { 24, 30, 60, 75, 120, 144, 240, Int32.MaxValue };

    public int TargetFps
    {
        get => _targetFps;
        set
        {
            int index = FpsLimits.IndexOf(i => (int)i == value);
            fpsOption.SetValue(index != -1 ? index : 2);
            _targetFps = value;
        }
    }
    public bool ChatDarkMode
    {
        get => _darkMode;
        set
        {
            darkOption.SetValue(value ? 1 : 0);
            _darkMode = value;
        }
    }

    private int _targetFps;
    private bool _darkMode;

    private GameOption fpsOption;
    private GameOption darkOption;

    public VideoOptions()
    {
        OptionManager optionManager = OptionManager.GetManager(file: "display.txt", managerFlags: OptionManagerFlags.IgnorePreset);
        fpsOption = new GameOptionBuilder()
            .Values(2, FpsLimits)
            .KeyName("Max Framerate", "Max Framerate")
            .Description("Maximum Framerate for the Application")
            .IOSettings(s => s.UnknownValueAction = ADEAnswer.Allow)
            .BindInt(i =>
            {
                optionManager.DelaySave(0);
                Application.targetFrameRate = _targetFps = i;
                Async.Schedule(() => Application.targetFrameRate = _targetFps, 1f);
            })
            .BuildAndRegister(optionManager);

        darkOption = new GameOptionBuilder()
            .AddBoolean(false)
            .KeyName("Dark Mode", "Dark Mode")
            .Description("Whether or not Chat will be dark")
            .IOSettings(s => s.UnknownValueAction = ADEAnswer.UseDefault)
            .BindBool(b =>
            {
                _darkMode = b;
                optionManager.DelaySave(0);
            })
            .BuildAndRegister(optionManager);
    }
}