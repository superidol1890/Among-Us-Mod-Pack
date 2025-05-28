using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.Roles.Internals.Enums;
using Lotus.Factions;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class NeutWitch : NeutralKillingBase
{
    // literally renamed Puppeteer
    private DateTime lastCheck = DateTime.Now;
    private List<PlayerControl> cursedPlayers;
    private Dictionary<byte, Remote<IndicatorComponent>> remotes = new();

    protected override void Setup(PlayerControl player) => cursedPlayers = new List<PlayerControl>();
    protected override void PostSetup() => remotes = new Dictionary<byte, Remote<IndicatorComponent>>();

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;

        cursedPlayers.Add(target);
        remotes.GetValueOrDefault(target.PlayerId)?.Delete();
        IndicatorComponent component = new SimpleIndicatorComponent("◆", new Color(0.36f, 0f, 0.58f), Game.InGameStates, MyPlayer);
        remotes[target.PlayerId] = target.NameModel().GetComponentHolder<IndicatorHolder>().Add(component);
        MyPlayer.RpcMark(target);
        return true;
    }

    [RoleAction(LotusActionType.FixedUpdate)]
    private void PuppeteerKillCheck()
    {
        double elapsed = (DateTime.Now - lastCheck).TotalSeconds;
        if (elapsed < ModConstants.RoleFixedUpdateCooldown) return;
        lastCheck = DateTime.Now;
        foreach (PlayerControl player in new List<PlayerControl>(cursedPlayers))
        {

            if (player.Data.IsDead)
            {
                RemovePuppet(player);
                continue;
            }
            List<PlayerControl> inRangePlayers = player.GetPlayersInAbilityRangeSorted().Where(p => p.Relationship(MyPlayer) is not Relation.FullAllies).ToList();
            if (inRangePlayers.Count == 0) continue;
            player.RpcMurderPlayer(inRangePlayers.GetRandom(), true);
            RemovePuppet(player);
        }


        cursedPlayers.Where(p => p.Data.IsDead).ToArray().Do(RemovePuppet);
    }

    private void RemovePuppet(PlayerControl puppet)
    {
        remotes.GetValueOrDefault(puppet.PlayerId)?.Delete();
        cursedPlayers.Remove(puppet);
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).OptionOverride(new IndirectKillCooldown(KillCooldown));
}