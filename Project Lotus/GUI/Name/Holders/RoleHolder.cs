using Lotus.GUI.Name.Components;

namespace Lotus.GUI.Name.Holders;

public class RoleHolder : ComponentHolder<RoleComponent>
{
    public RoleHolder(int line = 0, DisplayStyle displayStyle = DisplayStyle.LastOnly) : base(line, displayStyle)
    {
        Spacing = 1;
    }
}