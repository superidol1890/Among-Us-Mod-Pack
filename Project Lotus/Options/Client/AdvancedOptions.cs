using Lotus.Server.Patches;
using VentLib.Options;
using VentLib.Options.UI;
using VentLib.Options.IO;

namespace Lotus.Options.Client;

public class AdvancedOptions
{
    public AdvancedOptions()
    {
        OptionManager defaultManager = OptionManager.GetManager(file: "advanced.txt", managerFlags: OptionManagerFlags.IgnorePreset);
    }
}