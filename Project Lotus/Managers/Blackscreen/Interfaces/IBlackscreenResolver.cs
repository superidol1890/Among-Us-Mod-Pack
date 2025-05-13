using System;

namespace Lotus.Managers.Blackscreen.Interfaces;

public interface IBlackscreenResolver
{
    /// <summary>
    /// Returns if the Blackscren Resolver is currently active.
    /// </summary>
    bool Patching();

    /// <summary>
    /// This function is ran after the Meeting is destroyed.
    /// </summary>
    void OnMeetingDestroy();

    /// <summary>
    /// This function is ran in the postifx of ExileController.WrapUp.
    /// </summary>
    /// <param name="runOnFinish">Function that handles setting up the new round.</param>
    void FixBlackscreens(Action runOnFinish);
}