using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.Options;
using Lotus.Roles.GUI;
using Lotus.Roles.GUI.Interfaces;
using Lotus.RPC;
using VentLib;
using VentLib.Localization.Attributes;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options.UI;
using VentLib.Options.IO;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using static Lotus.Roles.RoleGroups.Crew.Bastion.BastionTranslations.BastionOptionTranslations;

namespace Lotus.Roles.RoleGroups.Crew;

public class Bastion : Engineer, IRoleUI
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Bastion));
    private int bombsPerRounds;

    // Here we can use the vent button as cooldown
    [NewOnSetup] private HashSet<int> bombedVents;

    private int currentBombs;
    private Remote<CounterComponent>? counterRemote;

    public RoleButton AbilityButton(IRoleButtonEditor abilityButton) => abilityButton
        .BindUses(() => currentBombs)
        .SetText(BastionTranslations.ButtonText)
        .SetSprite(() => LotusAssets.LoadSprite("Buttons/Crew/bastion_plant_bomb.png", 130, true));

    protected override void PostSetup()
    {
        if (bombsPerRounds == -1) return;
        CounterHolder counterHolder = MyPlayer.NameModel().GetComponentHolder<CounterHolder>();
        LiveString ls = new(() => RoleUtils.Counter(currentBombs, bombsPerRounds, ModConstants.Palette.GeneralColor2));
        counterRemote = counterHolder.Add(new CounterComponent(ls, [GameState.Roaming], ViewMode.Additive, MyPlayer));
        currentBombs = bombsPerRounds;
    }

    [RoleAction(LotusActionType.VentEntered, ActionFlag.GlobalDetector)]
    private void EnterVent(PlayerControl player, Vent vent, ActionHandle handle)
    {
        bool isBombed = bombedVents.Remove(vent.Id);
        log.Trace($"Bombed Vent Check: (player={player.name}, isBombed={isBombed})", "BastionAbility");
        if (isBombed) MyPlayer.InteractWith(player, CreateInteraction(player));
        else if (player.PlayerId == MyPlayer.PlayerId)
        {
            handle.Cancel();
            if (currentBombs == 0) return;
            currentBombs--;
            if (MyPlayer.IsModded()) Vents.FindRPC((uint)ModCalls.UpdateBastion)?.Send([MyPlayer.OwnerId], currentBombs);
            bombedVents.Add(vent.Id);
        }
    }

    [RoleAction(LotusActionType.RoundStart, ActionFlag.WorksAfterDeath)]
    private void RefreshBastion()
    {
        currentBombs = bombsPerRounds;
        bombedVents.Clear();
        if (MyPlayer.IsModded()) Vents.FindRPC((uint)ModCalls.UpdateBastion)?.Send([MyPlayer.OwnerId], currentBombs);
    }

    [RoleAction(LotusActionType.PlayerDeath)]
    private void ClearCounter() => counterRemote?.Delete();

    private IndirectInteraction CreateInteraction(PlayerControl deadPlayer)
    {
        return new IndirectInteraction(new FatalIntent(true, () => new BombedEvent(deadPlayer, MyPlayer)), this);
    }

    [ModRPC((uint)ModCalls.UpdateBastion, RpcActors.Host, RpcActors.NonHosts)]
    private static void RpcUpdateBastion(int bombsLeft)
    {
        Bastion? bastion = PlayerControl.LocalPlayer.PrimaryRole<Bastion>();
        if (bastion == null) return;
        bastion.currentBombs = bombsLeft;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Plant Bomb Cooldown", PlantBombCooldown)
                .BindFloat(v => VentCooldown = v)
                .Value(1f)
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.Allow)
                .AddFloatRange(2.5f, 120, 2.5f, 8, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Bombs per Round", BombsPerRound)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .AddIntRange(1, 20, 1, 0)
                .BindInt(i => bombsPerRounds = i)
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier)
        .RoleColor("#524f4d");

    [Localized(nameof(Bastion))]
    public static class BastionTranslations
    {
        [Localized(nameof(ButtonText))]
        public static string ButtonText = "Bomb";

        [Localized(ModConstants.Options)]
        public static class BastionOptionTranslations
        {
            [Localized(nameof(PlantBombCooldown))]
            public static string PlantBombCooldown = "Plant Bomb Cooldown";

            [Localized(nameof(BombsPerRound))]
            public static string BombsPerRound = "Bombs per Round";
        }
    }
}