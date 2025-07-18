﻿using System.Collections.Generic;
using System.Linq;
using EHR.AddOns.Impostor;
using EHR.Crewmate;
using static EHR.Options;
using static EHR.Utils;

namespace EHR.Impostor;

internal class Kamikaze : RoleBase
{
    private static List<byte> PlayerIdList = [];
    public static bool On;

    private static OptionItem MarkCD;
    private static OptionItem KamikazeLimitOpt;
    public static OptionItem KamikazeAbilityUseGainWithEachKill;
    private byte KamikazeId;

    public List<byte> MarkedPlayers = [];
    private static int Id => 643310;

    public override bool IsEnable => On;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Kamikaze);

        MarkCD = new FloatOptionItem(Id + 2, "KamikazeMarkCD", new(0f, 180f, 0.5f), 30f, TabGroup.ImpostorRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Kamikaze])
            .SetValueFormat(OptionFormat.Seconds);

        KamikazeLimitOpt = new IntegerOptionItem(Id + 3, "AbilityUseLimit", new(0, 20, 1), 1, TabGroup.ImpostorRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Kamikaze])
            .SetValueFormat(OptionFormat.Times);

        KamikazeAbilityUseGainWithEachKill = new FloatOptionItem(Id + 4, "AbilityUseGainWithEachKill", new(0f, 5f, 0.1f), 1f, TabGroup.ImpostorRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Kamikaze])
            .SetValueFormat(OptionFormat.Times);
    }

    public override void Init()
    {
        PlayerIdList = [];
        MarkedPlayers.Clear();
        On = false;
        KamikazeId = byte.MaxValue;
    }

    public override void Add(byte playerId)
    {
        PlayerIdList.Add(playerId);
        MarkedPlayers = [];
        playerId.SetAbilityUseLimit(KamikazeLimitOpt.GetFloat());
        On = true;
        KamikazeId = playerId;
    }

    public override void Remove(byte playerId)
    {
        PlayerIdList.Remove(playerId);
    }

    public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return false;

        if (killer.GetAbilityUseLimit() < 1) return true;

        return killer.CheckDoubleTrigger(target, () =>
        {
            MarkedPlayers.Add(target.PlayerId);
            killer.SetKillCooldown(MarkCD.GetFloat());
            killer.RpcRemoveAbilityUse();
        });
    }

    public override void OnGlobalFixedUpdate(PlayerControl pc, bool lowLoad)
    {
        if (lowLoad || !On) return;

        foreach (byte kkId in PlayerIdList)
        {
            if (Main.PlayerStates[kkId].Role is Kamikaze { IsEnable: true } kk)
            {
                PlayerControl kamikazePc = GetPlayerById(kk.KamikazeId);

                if (kamikazePc == null)
                {
                    LateTask.New(() => PlayerIdList.Remove(kkId), 0.001f, log: false);
                    continue;
                }

                if (kamikazePc.IsAlive() || kk.MarkedPlayers.Count == 0) continue;

                foreach (byte id in kk.MarkedPlayers)
                {
                    PlayerControl victim = GetPlayerById(id);
                    if (victim == null || !victim.IsAlive()) continue;

                    if (GameStates.IsInTask && !ExileController.Instance)
                        victim.Suicide(PlayerState.DeathReason.Kamikazed, kamikazePc);
                    else
                    {
                        victim.SetRealKiller(kamikazePc);
                        PlayerState state = Main.PlayerStates[victim.PlayerId];
                        state.deathReason = PlayerState.DeathReason.Kamikazed;
                        state.SetDead();
                        Medic.IsDead(victim);

                        if (kamikazePc.Is(CustomRoles.Damocles))
                            Damocles.OnMurder(kamikazePc.PlayerId);

                        IncreaseAbilityUseLimitOnKill(kamikazePc);

                        victim.RpcExileV2();
                        AfterPlayerDeathTasks(victim, true);
                    }
                }

                Logger.Info($"Murder {kamikazePc.GetRealName()}'s targets: {string.Join(", ", kk.MarkedPlayers.Select(x => GetPlayerById(x).GetNameWithRole()))}", "Kamikaze");
                kk.MarkedPlayers.Clear();
            }
        }
    }
}