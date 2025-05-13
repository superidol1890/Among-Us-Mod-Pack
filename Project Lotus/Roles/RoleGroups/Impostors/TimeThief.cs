using System.Collections.Generic;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.UI;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Impostors;

public class TimeThief : Vanilla.Impostor
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Vampiress));
    private int kills;
    private int meetingTimeSubtractor;
    private int minimumVotingTime;
    private bool returnTimeAfterDeath;

    private List<IRemote>? discussionRemote;
    private List<IRemote>? votingRemote;

    [UIComponent(UI.Counter)]
    public string TimeStolenCounter() => RoleUtils.Counter(kills * meetingTimeSubtractor + "s", color: RoleColor);

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        var flag = base.TryKill(target);
        if (flag)
            kills++;
        return flag;
    }

    [RoleAction(LotusActionType.ReportBody, ActionFlag.WorksAfterDeath | ActionFlag.GlobalDetector, priority: API.Priority.Low)]
    private void TimeThiefSubtractMeetingTime()
    {
        discussionRemote?.ForEach(d => d.Delete());
        votingRemote?.ForEach(v => v.Delete());
        if (!MyPlayer.IsAlive() && returnTimeAfterDeath) return;
        int discussionTime = AUSettings.DiscussionTime();
        int votingTime = AUSettings.VotingTime();

        int totalStolenTime = meetingTimeSubtractor * kills;

        // Total Meeting Time - Stolen Time = Remaining Meeting Time
        int modifiedDiscussionTime = discussionTime - totalStolenTime;

        if (modifiedDiscussionTime < 0)
        {
            totalStolenTime = -modifiedDiscussionTime;
            modifiedDiscussionTime = 1;
        }

        int modifiedVotingTime = Mathf.Clamp(votingTime - totalStolenTime, minimumVotingTime, votingTime);

        log.Debug($"{MyPlayer.name} | Time Thief | Meeting Time: {modifiedDiscussionTime} | Voting Time: {modifiedVotingTime}", "TimeThiefStolen");

        discussionRemote = new List<IRemote>();
        votingRemote = new List<IRemote>();
        Players.GetPlayers().ForEach(p =>
        {
            discussionRemote.Add(Game.MatchData.Roles.AddOverride(p.PlayerId, new GameOptionOverride(Override.DiscussionTime, modifiedDiscussionTime)));
            votingRemote.Add(Game.MatchData.Roles.AddOverride(p.PlayerId, new GameOptionOverride(Override.VotingTime, modifiedVotingTime)));
        });

    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Meeting Time Stolen", Translations.Options.MeetingTimeStolen)
                .Bind(v => meetingTimeSubtractor = (int)v)
                .AddIntRange(5, 120, 5, 4, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Minimum Voting Time", Translations.Options.MinVotingTime)
                .Bind(v => minimumVotingTime = (int)v)
                .AddIntRange(5, 120, 5, 1, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Return Stolen Time After Death", Translations.Options.ReturnTimeAfterDeath)
                .Bind(v => returnTimeAfterDeath = (bool)v)
                .AddBoolean()
                .Build());

    [Localized(nameof(TimeThief))]
    public static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(MeetingTimeStolen))]
            public static string MeetingTimeStolen = "Meeting Time Stolen";

            [Localized(nameof(MinVotingTime))]
            public static string MinVotingTime = "Minimum Voting Time";

            [Localized(nameof(ReturnTimeAfterDeath))]
            public static string ReturnTimeAfterDeath = "Return Stolen Time After Death";
        }
    }
}