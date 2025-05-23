﻿using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using EHR.Modules;
using Hazel;

namespace EHR.Neutral;

public class NoteKiller : RoleBase
{
    public static bool On;

    public static OptionItem AbilityCooldown;
    private static OptionItem MinLettersRevealed;
    private static OptionItem MaxLettersRevealed;
    private static OptionItem ClueShowDuration;
    private static OptionItem WinCondition;
    private static OptionItem NumPlayersToKill;
    private static OptionItem CanVent;
    private static OptionItem ImpostorVision;

    private static readonly string[] WinConditions =
    [
        "NKWC.LastStanding",
        "NKWC.XKills"
    ];

    private static readonly string[] Names =
    [
        "John", "Jane", "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Hank",
        "Ivy", "Jack", "Kate", "Liam", "Nina", "Oliver", "Penny", "Quinn", "Ryan",
        "Thomas", "Uma", "Victor", "William", "Yang", "Zach", "Zoe", "Xavier", "Zoey",
        "Abigail", "Alex", "Amelia", "Ava", "Bella", "Charlotte", "Chloe", "Daisy", "Emily",
        "Ella", "Evelyn", "Faith", "Florence", "Grace", "Hannah", "Isabella", "Isabelle",
        "Isabel", "Jasmine", "Jocelyn", "Julia", "Katherine", "Lily", "Madeline", "Madison",
        "Madelyn", "Margaret", "Maria", "Matilda", "Mia", "Mila", "Miranda", "Natalie",
        "Nora", "Olivia", "Penelope", "Piper", "Poppy", "Riley", "Rose", "Sophia", "Sofia",
        "Sophie", "Stella", "Sydney", "Taylor", "Victoria", "Violet", "Willow", "Xander"
    ];

    public static Dictionary<byte, string> RealNames = [];
    private static Dictionary<byte, string> ShownClues = [];
    private static long ShowClueEndTimeStamp;
    public static int Kills;
    public static bool CanGuess;

    private static byte NoteKillerID;

    public static bool CountsAsNeutralKiller => WinCondition?.GetValue() == 0;
    public static int NumKillsNeededToWin => NumPlayersToKill.GetInt();

    public override bool IsEnable => On;

    public override void SetupCustomOption()
    {
        StartSetup(645950, true)
            .AutoSetupOption(ref AbilityCooldown, 15f, new FloatValueRule(0f, 90f, 0.5f), OptionFormat.Seconds)
            .AutoSetupOption(ref MinLettersRevealed, 1, new IntegerValueRule(1, Names.Max(x => x.Length), 1))
            .AutoSetupOption(ref MaxLettersRevealed, 4, new IntegerValueRule(1, Names.Max(x => x.Length), 1))
            .AutoSetupOption(ref ClueShowDuration, 5, new IntegerValueRule(0, 30, 1), OptionFormat.Seconds)
            .AutoSetupOption(ref WinCondition, 0, WinConditions)
            .AutoSetupOption(ref NumPlayersToKill, 2, new IntegerValueRule(0, 14, 1), overrideParent: WinCondition)
            .AutoSetupOption(ref CanVent, true)
            .AutoSetupOption(ref ImpostorVision, true);
    }

    public override void Init()
    {
        On = false;

        RealNames = [];
        ShownClues = [];
        ShowClueEndTimeStamp = 0;
        Kills = 0;

        LateTask.New(() =>
        {
            List<string> names = Names.ToList();

            foreach (PlayerControl pc in Main.AllAlivePlayerControls)
            {
                if (pc.Is(CustomRoles.NoteKiller)) continue;
                string name = names.RandomElement();
                RealNames[pc.PlayerId] = name;
                names.Remove(name);
                Logger.Info($"{pc.GetRealName()}'s real name is {name}", "NoteKiller.Init");
            }
        }, 10f, log: false);
    }

    public override void Add(byte playerId)
    {
        On = true;
        NoteKillerID = playerId;
        CanGuess = true;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return false;
    }

    public override void ApplyGameOptions(IGameOptions opt, byte id)
    {
        opt.SetVision(ImpostorVision.GetBool());
    }

    public override bool CanUseImpostorVentButton(PlayerControl pc)
    {
        return CanVent.GetBool();
    }

    public override void OnPet(PlayerControl pc)
    {
        Dictionary<byte, List<int>> revealedPositions = [];
        int numLettersRevealed = IRandom.Instance.Next(MinLettersRevealed.GetInt(), MaxLettersRevealed.GetInt() + 1);

        for (var i = 0; i < numLettersRevealed; i++)
        {
            foreach (KeyValuePair<byte, string> kvp in RealNames)
            {
                IEnumerable<int> range = Enumerable.Range(0, Names.Max(x => x.Length) - 1);
                bool hasExceptions = revealedPositions.TryGetValue(kvp.Key, out List<int> exceptions);
                if (hasExceptions) range = range.Except(exceptions);

                int position = range.RandomElement();

                if (!hasExceptions) revealedPositions[kvp.Key] = [position];
                else exceptions.Add(position);
            }
        }

        IEnumerable<(byte ID, List<int> Positions, string Name, PlayerControl Player)> datas = revealedPositions.Join(
            RealNames, x => x.Key, x => x.Key, (x, y) => (
                ID: x.Key,
                Positions: x.Value,
                Name: y.Value,
                Player: y.Key.GetPlayer()));

        foreach ((byte ID, List<int> Positions, string Name, PlayerControl Player) data in datas)
        {
            if (data.Player == null || !data.Player.IsAlive())
            {
                RealNames.Remove(data.ID);
                continue;
            }

            var clue = new char[data.Name.Length];
            Loop.Times(data.Name.Length, i => clue[i] = data.Positions.Contains(i) ? data.Name[i] : '_');

            string shownClue = new(clue);
            ShownClues[data.ID] = shownClue;
            Utils.SendRPC(CustomRPC.SyncRoleData, NoteKillerID, 1, data.ID, shownClue);
        }

        ShowClueEndTimeStamp = Utils.TimeStamp + ClueShowDuration.GetInt();
        Utils.SendRPC(CustomRPC.SyncRoleData, NoteKillerID, 2, ShowClueEndTimeStamp);
        Utils.NotifyRoles(SpecifySeer: pc);
    }

    public override void OnFixedUpdate(PlayerControl pc)
    {
        if (!GameStates.IsInTask || ExileController.Instance || pc == null || !pc.IsAlive() || ShowClueEndTimeStamp == 0 || ShownClues.Count == 0) return;

        if (Utils.TimeStamp >= ShowClueEndTimeStamp)
        {
            ShowClueEndTimeStamp = 0;
            ShownClues.Clear();
            Utils.SendRPC(CustomRPC.SyncRoleData, NoteKillerID, 3);
            Utils.NotifyRoles(SpecifySeer: pc);
        }
    }

    public override void AfterMeetingTasks()
    {
        CanGuess = true;
    }

    public void ReceiveRPC(MessageReader reader)
    {
        switch (reader.ReadPackedInt32())
        {
            case 1:
                ShownClues[reader.ReadByte()] = reader.ReadString();
                break;
            case 2:
                ShowClueEndTimeStamp = long.Parse(reader.ReadString());
                break;
            case 3:
                ShowClueEndTimeStamp = 0;
                ShownClues.Clear();
                break;
        }
    }

    public override string GetSuffix(PlayerControl seer, PlayerControl target, bool hud = false, bool meeting = false)
    {
        if (seer.PlayerId == target.PlayerId && CustomRoles.NoteKiller.RoleExist() && seer.PlayerId != NoteKillerID && !meeting && RealNames.TryGetValue(seer.PlayerId, out string ownName))
            return MeetingStates.FirstMeeting ? string.Format(Translator.GetString("NoteKiller.OthersSelfSuffix"), CustomRoles.NoteKiller.ToColoredString(), ownName) : string.Format(Translator.GetString("NoteKiller.OthersSelfSuffixShort"), ownName);

        if (seer.PlayerId != NoteKillerID || meeting || ShowClueEndTimeStamp == 0 || ShownClues.Count == 0) return string.Empty;
        if (ShownClues.TryGetValue(target.PlayerId, out string clue)) return clue;
        return seer.PlayerId == target.PlayerId ? $"\u25a9 ({ShowClueEndTimeStamp - Utils.TimeStamp}s)" : string.Empty;
    }
}