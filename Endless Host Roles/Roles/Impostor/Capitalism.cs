﻿using System.Collections.Generic;
using static EHR.Options;

namespace EHR.Impostor;

internal class Capitalism : RoleBase
{
    private static readonly Dictionary<byte, int> CapitalismAddTask = [];
    public static readonly Dictionary<byte, int> CapitalismAssignTask = [];
    public static bool On;
    public override bool IsEnable => On;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(16600, TabGroup.ImpostorRoles, CustomRoles.Capitalism);

        CapitalismKillCooldown = new FloatOptionItem(16611, "KillCooldown", new(2.5f, 60f, 0.5f), 25f, TabGroup.ImpostorRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Capitalism])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Add(byte playerId)
    {
        On = true;
    }

    public override void Init()
    {
        On = false;
    }

    public override void SetButtonTexts(HudManager hud, byte id)
    {
        hud.KillButton?.OverrideText(Translator.GetString("CapitalismButtonText"));
    }

    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = CapitalismKillCooldown.GetFloat();
    }

    public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        return killer.CheckDoubleTrigger(target, () =>
        {
            CapitalismAddTask.TryAdd(target.PlayerId, 0);
            CapitalismAddTask[target.PlayerId]++;
            CapitalismAssignTask.TryAdd(target.PlayerId, 0);
            CapitalismAssignTask[target.PlayerId]++;
            Logger.Info($"{killer.GetRealName()} added a task for: {target.GetRealName()}", "Capitalism Add Task");
        });
    }

    public static bool AddTaskForPlayer(PlayerControl player)
    {
        if (CapitalismAddTask.TryGetValue(player.PlayerId, out int amount))
        {
            TaskState taskState = player.GetTaskState();
            taskState.AllTasksCount += amount;
            CapitalismAddTask.Remove(player.PlayerId);
            taskState.CompletedTasksCount++;
            player.RpcResetTasks(false);
            player.SyncSettings();
            Utils.NotifyRoles(SpecifySeer: player, SpecifyTarget: player);
            return false;
        }

        return true;
    }
}