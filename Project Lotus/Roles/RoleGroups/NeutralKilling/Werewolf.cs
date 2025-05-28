using System.Linq;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Internals.Attributes;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.GUI;
using Lotus.Roles.GUI.Interfaces;
using UnityEngine;
using VentLib.Localization.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.RPC;
using VentLib;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options.UI;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Werewolf : NeutralKillingBase, IRoleUI
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Werewolf));
    private bool rampaging;
    private bool canVentNormally;
    private bool canVentDuringRampage;

    [UIComponent(UI.Cooldown)]
    private Cooldown rampageDuration;

    [UIComponent(UI.Cooldown)]
    private Cooldown rampageCooldown;

    public RoleButton KillButton(IRoleButtonEditor killButton) =>
        killButton.SetText(Translations.KillButtonText).BindUses(() => 0);

    public RoleButton PetButton(IRoleButtonEditor petButton) =>
        petButton
            .BindCooldown(rampageCooldown)
            .SetText(Translations.PetButtonText)
            .SetSprite(() => LotusAssets.LoadSprite("Buttons/Neut/werewolf_rampage.png", 130, true));

    [RoleAction(LotusActionType.RoundStart)]
    private void OnRoundStart(bool gameStart)
    {
        if (MyPlayer.AmOwner)
        {
            UIManager.KillButton.BindUses(() => 0);
            UIManager.PetButton.BindCooldown(rampageCooldown);
        } else Vents.FindRPC((uint)ModCalls.UpdateWerewolf)?.Send([MyPlayer.OwnerId], false, gameStart);
        rampageCooldown.Start(gameStart ? 10 : float.MinValue);
    }

    protected override void PostSetup()
    {
        base.PostSetup();
        MyPlayer.NameModel().GetComponentHolder<CooldownHolder>()[1].SetPrefix(RoleColor.Colorize(Translations.Rampage + " "));
    }

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => rampaging && base.TryKill(target);

    [RoleAction(LotusActionType.OnPet)]
    private void EnterRampage()
    {
        if (rampageDuration.NotReady() || rampageCooldown.NotReady()) return;
        log.Trace($"{MyPlayer.GetNameWithRole()} Starting Rampage");
        if (MyPlayer.AmOwner)
        {
            UIManager.KillButton.BindUses(() => -1);
            UIManager.PetButton.BindCooldown(rampageDuration);
        } else if (MyPlayer.IsModded()) Vents.FindRPC((uint)ModCalls.UpdateWerewolf)?.Send([MyPlayer.OwnerId], true, false);
        rampaging = true;
        rampageDuration.Start();
        Async.Schedule(ExitRampage, rampageDuration.Duration);
    }

    [RoleAction(LotusActionType.RoundEnd)]
    private void ExitRampage()
    {
        log.Trace($"{MyPlayer.GetNameWithRole()} Ending Rampage");
        if (MyPlayer.AmOwner)
        {
            UIManager.KillButton.BindUses(() => 0);
            UIManager.PetButton.BindCooldown(rampageCooldown);
        } else if (MyPlayer.IsModded()) Vents.FindRPC((uint)ModCalls.UpdateWerewolf)?.Send([MyPlayer.OwnerId], false, false);
        rampaging = false;
        rampageCooldown.Start();
        if (!canVentNormally && MyPlayer.walkingToVent | MyPlayer.inVent) ExitCurrentVent();

    }

    private void ExitCurrentVent()
    {
        int ventId;

        ISystemType ventilation = ShipStatus.Instance.Systems[SystemTypes.Ventilation];
        if (ventilation.TryCast(out VentilationSystem ventilationSystem))
        {
            if (ventilationSystem.PlayersInsideVents.TryGetValue(MyPlayer.PlayerId, out byte byteId)) ventId = byteId;
            else ventId = Object.FindObjectsOfType<Vent>().ToList().GetRandom().Id;
        }
        else ventId = Object.FindObjectsOfType<Vent>().ToList().GetRandom().Id;

        MyPlayer.MyPhysics.RpcBootFromVent(ventId);
    }

    [ModRPC((uint)ModCalls.UpdateWerewolf, RpcActors.Host, RpcActors.NonHosts)]
    private static void RpcUpdateWerewolf(bool isInRampage, bool isRoundOne)
    {
        Werewolf? werewolf = PlayerControl.LocalPlayer.PrimaryRole<Werewolf>();
        if (werewolf == null) return;
        werewolf.rampaging = isInRampage;
        if (isInRampage)
        {
            werewolf.UIManager.KillButton.BindUses(() => -1);
            werewolf.UIManager.PetButton.BindCooldown(werewolf.rampageDuration);
            werewolf.rampageDuration.Start();
        }
        else
        {

            werewolf.UIManager.KillButton.BindUses(() => 0);
            werewolf.UIManager.PetButton.BindCooldown(werewolf.rampageCooldown);
            werewolf.rampageCooldown.Start(isRoundOne ? 10 : float.MinValue);
        }
    }

    public override bool CanVent() => canVentNormally || rampaging && canVentDuringRampage;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Rampage Kill Cooldown", Translations.Options.RampageKillCooldown)
                .AddFloatRange(1f, 60f, 2.5f, 2, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => KillCooldown = f)
                .Build())
            .SubOption(sub => sub
                .KeyName("Rampage Cooldown", Translations.Options.RampageCooldown)
                .AddFloatRange(5f, 120f, 2.5f, 14, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(rampageCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub
                .KeyName("Rampage Duration", Translations.Options.RampageDuration)
                .AddFloatRange(5f, 120f, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(rampageDuration.SetDuration)
                .Build())
            .SubOption(sub => sub
                .KeyName("Can Vent Normally", Translations.Options.CanVentNormally)
                .AddBoolean(false)
                .BindBool(b => canVentNormally = b)
                .ShowSubOptionPredicate(o => !(bool)o)
                .SubOption(sub2 => sub2
                    .KeyName("Can Vent in Rampage", Translations.Options.CanVentInRampage)
                    .BindBool(b => canVentDuringRampage = b)
                    .AddBoolean()
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        RoleAbilityFlag flags = RoleAbilityFlag.CannotSabotage | RoleAbilityFlag.UsesPet;
        if (!(canVentNormally || canVentDuringRampage)) flags |= RoleAbilityFlag.CannotVent;

        return base.Modify(roleModifier)
            .RoleAbilityFlags(flags)
            .RoleColor(new Color(0.66f, 0.4f, 0.16f));
    }

    [Localized(nameof(Werewolf))]
    public static class Translations
    {
        [Localized(nameof(PetButtonText))] public static string PetButtonText = "Rampage";
        [Localized(nameof(KillButtonText))] public static string KillButtonText = "Maul";
        [Localized(nameof(Rampage))] public static string Rampage = "RAMPAGING";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(RampageKillCooldown))]
            public static string RampageKillCooldown = "Rampage Kill Cooldown";

            [Localized(nameof(RampageCooldown))]
            public static string RampageCooldown = "Rampage Cooldown";

            [Localized(nameof(RampageDuration))]
            public static string RampageDuration = "Rampage Duration";

            [Localized(nameof(CanVentNormally))]
            public static string CanVentNormally = "Can Vent Normally";

            [Localized(nameof(CanVentInRampage))]
            public static string CanVentInRampage = "Can Vent in Rampage";
        }
    }
}