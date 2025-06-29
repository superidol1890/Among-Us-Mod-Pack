using System.Linq;

namespace MiraAPI.GameOptions;

#pragma warning disable CA1852
internal class ModifierOptionGroup : AbstractOptionGroup
#pragma warning restore CA1852
{
    public override string GroupName { get; }

    public ModifierOptionGroup(string name, IModdedOption[] options, params AbstractOptionGroup[] groups)
    {
        GroupName = name;
        Options.AddRange(options);
        Options.AddRange(groups.SelectMany(x=>x.Options));
    }
}
