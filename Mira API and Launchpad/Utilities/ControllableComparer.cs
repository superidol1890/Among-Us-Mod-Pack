using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.Utilities;

internal sealed class ControllableComparer<T> : IComparer<T> where T : IComparable
{
    private readonly T[] _forcedToBottom;
    private readonly T[] _forcedToTop;
    private readonly IComparer<T> _fallbackComparer;
    public ControllableComparer(T[] forcedToBottom, T[] forcedToTop, IComparer<T> fallbackComparer)
    {
        _forcedToBottom = forcedToBottom;
        _forcedToTop = forcedToTop;
        _fallbackComparer = fallbackComparer;
    }

    /// <inheritdoc/>
    public int Compare(T? x, T? y)
    {
        if ((_forcedToBottom.Contains(x) && _forcedToBottom.Contains(y)) || (_forcedToTop.Contains(x) && _forcedToTop.Contains(y)))
            return _fallbackComparer.Compare(x, y);

        if (_forcedToBottom.Contains(x))
            return 1;
        if (_forcedToBottom.Contains(y))
            return -1;

        if (_forcedToTop.Contains(x))
            return -1;
        if (_forcedToTop.Contains(y))
            return 1;

        return _fallbackComparer.Compare(x, y);
    }
}
