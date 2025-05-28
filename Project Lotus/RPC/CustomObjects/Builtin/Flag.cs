using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace Lotus.RPC.CustomObjects.Builtin;
public sealed class RedFlag : Flag
{
    public RedFlag(Vector2 position) : base(position)
    {
        CreateNetObject("<size=100%><font=\"VCR SDF\"><line-height=67%><#000000>\u2588<#ff0000>\u2588<#ff0000>\u2588<#ff0000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<br><#000000>\u2588<#ff0000>\u2588<#ff0000>\u2588<#ff0000>\u2588<#ff0000>\u2588<#ff0000>\u2588<br><#000000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<#ff0000>\u2588<#ff0000>\u2588<#ff0000>\u2588<br><#000000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<br><#000000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<br><#000000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<br></line-height></size>", position);
    }
}

public sealed class BlueFlag : Flag
{
    public BlueFlag(Vector2 position): base(position)
    {
        CreateNetObject("<size=100%><font=\"VCR SDF\"><line-height=67%><#000000>\u2588<#0000ff>\u2588<#0000ff>\u2588<#0000ff>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<br><#000000>\u2588<#0000ff>\u2588<#0000ff>\u2588<#0000ff>\u2588<#0000ff>\u2588<#0000ff>\u2588<br><#000000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<#0000ff>\u2588<#0000ff>\u2588<#0000ff>\u2588<br><#000000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<br><#000000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<br><#000000>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<alpha=#00>\u2588<br></line-height></size>", position);
    }
}

public class Flag : CustomNetObject
{
    protected Vector2 startPosition;

    public Flag(Vector2 position)
    {
        startPosition = position;
    }

    public void Return() => SnapTo(startPosition);
}