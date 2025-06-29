using HarmonyLib;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Voting;

[HarmonyPatch(typeof(DummyBehaviour), nameof(DummyBehaviour.Update))]
internal static class DummyBehaviourPatch
{
    public static void Postfix(DummyBehaviour __instance)
    {
        if (MeetingHud.Instance && __instance.myPlayer.GetVoteData())
        {
            __instance.voted = __instance.myPlayer.GetVoteData().VotesRemaining == 0;
        }
    }
}
