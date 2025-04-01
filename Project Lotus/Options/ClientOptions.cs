using Lotus.Options.Client;
using VentLib.Utilities.Attributes;

namespace Lotus.Options;

[LoadStatic]
public class ClientOptions
{
    public static SoundOptions SoundOptions = new();

    public static VideoOptions VideoOptions = new();

    public static AdvancedOptions AdvancedOptions = new();


}