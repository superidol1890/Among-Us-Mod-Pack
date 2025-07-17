﻿using System.Collections.Generic;
using AmongUs.GameOptions;
using EHR.Modules;
using static EHR.Options;
using static EHR.Translator;

namespace EHR.Neutral;

public class Postman : RoleBase
{
    private const int Id = 641400;
    public static List<byte> PlayerIdList = [];

    private static OptionItem KillCooldown;
    private static OptionItem CanVent;
    private static OptionItem HasImpostorVision;
    private static OptionItem DieWhenTargetDies;
    private static OptionItem HasArrowsToTargets;

    public bool IsFinished;
    private byte PostmanId;
    public byte Target;
    private List<byte> WereTargets = [];

    public override bool IsEnable => PlayerIdList.Count > 0;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Postman);

        KillCooldown = new FloatOptionItem(Id + 10, "DeliverCooldown", new(0f, 180f, 0.5f), 3f, TabGroup.NeutralRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Postman])
            .SetValueFormat(OptionFormat.Seconds);

        CanVent = new BooleanOptionItem(Id + 11, "CanVent", false, TabGroup.NeutralRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Postman]);

        HasImpostorVision = new BooleanOptionItem(Id + 13, "ImpostorVision", false, TabGroup.NeutralRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Postman]);

        DieWhenTargetDies = new BooleanOptionItem(Id + 12, "PostmanDiesWhenTargetDies", false, TabGroup.NeutralRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Postman]);

        HasArrowsToTargets = new BooleanOptionItem(Id + 14, "PostmanHasArrowsToTargets", true, TabGroup.NeutralRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Postman]);
    }

    public override void Init()
    {
        PlayerIdList = [];
        Target = byte.MaxValue;
        IsFinished = false;
        WereTargets = [];
    }

    public override void Add(byte playerId)
    {
        PlayerIdList.Add(playerId);
        PostmanId = playerId;
        LateTask.New(SetNewTarget, 8f, "Set Postman First Target");

        Target = byte.MaxValue;
        IsFinished = false;
        WereTargets = [];
    }

    public override void Remove(byte playerId)
    {
        PlayerIdList.Remove(playerId);
    }

    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    }

    public override void ApplyGameOptions(IGameOptions opt, byte id)
    {
        opt.SetVision(HasImpostorVision.GetBool());
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return !IsFinished;
    }

    public override bool CanUseImpostorVentButton(PlayerControl pc)
    {
        return CanVent.GetBool();
    }

    public static void CheckAndResetTargets(PlayerControl deadPc, bool isDeath = false)
    {
        foreach (byte id in PlayerIdList)
        {
            PlayerControl pc = Utils.GetPlayerById(id);
            if (pc == null || !pc.IsAlive()) continue;

            if (Main.PlayerStates[id].Role is Postman { IsEnable: true } pm && pm.Target == deadPc.PlayerId)
            {
                if (isDeath && DieWhenTargetDies.GetBool())
                    pc.Suicide();
                else
                {
                    pm.SetNewTarget();
                    if (!isDeath) continue;

                    pm.NotifyPostman(Utils.GetPlayerById(id), GetString("PostmanTargetDied"));
                }
            }
        }
    }

    private void SetNewTarget()
    {
        if (!IsEnable) return;

        TargetArrow.RemoveAllTarget(PostmanId);

        var tempTarget = byte.MaxValue;

        foreach (PlayerControl pc in Main.AllAlivePlayerControls)
        {
            if (WereTargets.Contains(pc.PlayerId) || pc.Is(CustomRoles.Postman)) continue;

            tempTarget = pc.PlayerId;
            break;
        }

        if (tempTarget == byte.MaxValue)
        {
            IsFinished = true;
            Target = byte.MaxValue;
            SendRPC();
            return;
        }

        Target = tempTarget;
        WereTargets.Add(Target);
        SendRPC();

        if (HasArrowsToTargets.GetBool())
            TargetArrow.Add(PostmanId, Target);
    }

    private void SendRPC()
    {
        Utils.SendRPC(CustomRPC.SyncPostman, PostmanId, Target, IsFinished);
    }

    public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (!IsEnable) return false;
        if (killer == null) return false;
        if (target == null) return false;
        if (IsFinished) return false;

        if (Target == byte.MaxValue)
        {
            SetNewTarget();
            return false;
        }

        if (target.PlayerId == Target)
        {
            SetNewTarget();
            killer.SetKillCooldown();
            NotifyPostman(killer, GetString("PostmanCorrectDeliver"));
        }
        else
            killer.Suicide();

        return false;
    }

    private void NotifyPostman(PlayerControl pc, string baseText)
    {
        if (!IsEnable) return;

        var sb = new StringBuilder();

        sb.AppendLine(baseText);
        sb.Append(!IsFinished ? string.Format(GetString("PostmanGetNewTarget"), Utils.GetPlayerById(Target).GetRealName()) : GetString("PostmanDone"));

        pc.Notify(sb.ToString());
    }

    private static string GetHudText(PlayerControl pc)
    {
        if (Main.PlayerStates[pc.PlayerId].Role is not Postman { IsEnable: true } pm) return string.Empty;
        return !pm.IsFinished ? string.Format(GetString("PostmanTarget"), Utils.GetPlayerById(pm.Target).GetRealName()) + TargetArrow.GetAllArrows(pc.PlayerId) : GetString("PostmanDone");
    }

    public override string GetSuffix(PlayerControl seer, PlayerControl target, bool hud = false, bool meeting = false)
    {
        if (hud) return GetHudText(seer);
        if (seer.IsModdedClient() || seer.PlayerId != target.PlayerId || Main.PlayerStates[seer.PlayerId].Role is not Postman { IsEnable: true } pm) return string.Empty;
        return !pm.IsFinished ? string.Format(GetString("PostmanTarget"), Utils.GetPlayerById(pm.Target).GetRealName()) + TargetArrow.GetAllArrows(seer.PlayerId) : "<color=#00ff00>✓</color>";
    }

    public override void SetButtonTexts(HudManager hud, byte id)
    {
        hud.KillButton?.OverrideText(GetString("PostmanKillButtonText"));
    }
}