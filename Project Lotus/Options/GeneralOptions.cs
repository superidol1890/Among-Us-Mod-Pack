using System.Collections.Generic;
using Lotus.Options.General;
using VentLib.Localization.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities.Attributes;
using VentLib.Options;
using System.Linq;
using VentLib.Utilities.Extensions;

namespace Lotus.Options;

[Localized(ModConstants.Options)]
[LoadStatic]
public static class GeneralOptions
{
    public static readonly OptionManager StandardOptionManager = OptionManager.GetManager(file: "standard.txt", managerFlags: OptionManagerFlags.SyncOverRpc);
    public static readonly OptionManager CaptureOptionManager = OptionManager.GetManager(file: "ctf.txt", managerFlags: OptionManagerFlags.SyncOverRpc);
    public static readonly OptionManager ColorwarsOptionManager = OptionManager.GetManager(file: "colorwars.txt", managerFlags: OptionManagerFlags.SyncOverRpc);

    public static readonly AdminOptions AdminOptions;
    public static readonly DebugOptions DebugOptions;
    public static readonly GameplayOptions GameplayOptions;
    public static readonly MayhemOptions MayhemOptions;
    public static readonly MeetingOptions MeetingOptions;
    public static readonly MiscellaneousOptions MiscellaneousOptions;
    public static readonly SabotageOptions SabotageOptions;

    public static readonly List<GameOption> StandardOptions = new();

    static GeneralOptions()
    {
        AdminOptions = new AdminOptions();
        // StandardOptions.AddRange(AdminOptions.AllOptions);

        GameplayOptions = new GameplayOptions();
        StandardOptions.AddRange(GameplayOptions.AllOptions);

        SabotageOptions = new SabotageOptions();
        StandardOptions.AddRange(SabotageOptions.AllOptions);

        MeetingOptions = new MeetingOptions();
        StandardOptions.AddRange(MeetingOptions.AllOptions);

        MayhemOptions = new MayhemOptions();
        StandardOptions.AddRange(MayhemOptions.AllOptions);

        MiscellaneousOptions = new MiscellaneousOptions();
        // StandardOptions.AddRange(MiscellaneousOptions.AllOptions);

        DebugOptions = new DebugOptions();
        // StandardOptions.AddRange(DebugOptions.AllOptions);

        StandardOptions.AddRange(RoleOptions.LoadMadmateOptions().AllOptions);
        StandardOptions.AddRange(RoleOptions.LoadNeutralOptions().AllOptions);
        StandardOptions.AddRange(RoleOptions.LoadSubroleOptions().AllOptions);

        StandardOptions.Where(o => !o.Attributes.ContainsKey("Title")).ForEach(o => StandardOptionManager.Register(o, OptionLoadMode.LoadOrCreate));
    }
}