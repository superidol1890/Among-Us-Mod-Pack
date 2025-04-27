using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;
using TownOfUs.CrewmateRoles.InvestigatorMod;
using TownOfUs.CrewmateRoles.TrapperMod;
using System.Collections.Generic;
using TownOfUs.CrewmateRoles.DeputyMod;
using TownOfUs.CrewmateRoles.ImitatorMod;
using TownOfUs.CrewmateRoles.JailorMod;
using TownOfUs.CrewmateRoles.MayorMod;
using TownOfUs.CrewmateRoles.PoliticianMod;
using TownOfUs.CrewmateRoles.ProsecutorMod;
using TownOfUs.CrewmateRoles.SwapperMod;
using TownOfUs.CrewmateRoles.VigilanteMod;
using TownOfUs.ImpostorRoles.BlackmailerMod;
using TownOfUs.ImpostorRoles.HypnotistMod;
using TownOfUs.Modifiers.AssassinMod;
using TownOfUs.NeutralRoles.DoomsayerMod;
using TownOfUs.CrewmateRoles.ClericMod;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class AddMeetingButtons
    {
        public static void Prefix(MeetingHud __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Cleric))
            {
                var clerRole = (Cleric)role;
                foreach (var (id, effects) in clerRole.CleansedPlayers)
                {
                    CleansePlayer(id, effects);
                    if (PlayerControl.LocalPlayer == clerRole.Player)
                    {
                        string text = "Cleansed effects on " + Utils.PlayerById(id).name + ":";
                        foreach (var effect in effects) text += " " + effect.ToString() + ",";
                        text = text.Remove(text.Length - 1, 1);
                        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, text);
                    }
                }
            }
            foreach (var bytes in StartImitate.ImitatingPlayers)
            {
                var imi = Utils.PlayerById(bytes);
                if (!imi.Is(Faction.Impostors))
                {
                    List<RoleEnum> trappedPlayers = null;
                    Dictionary<byte, List<RoleEnum>> seenPlayers = null;
                    PlayerControl confessingPlayer = null;
                    PlayerControl jailedPlayer = null;

                    if (PlayerControl.LocalPlayer == imi)
                    {
                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Investigator)) Footprint.DestroyAll(Role.GetRole<Investigator>(PlayerControl.LocalPlayer));

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Engineer))
                        {
                            var engineerRole = Role.GetRole<Engineer>(PlayerControl.LocalPlayer);
                            Object.Destroy(engineerRole.UsesText);
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Tracker))
                        {
                            var trackerRole = Role.GetRole<Tracker>(PlayerControl.LocalPlayer);
                            trackerRole.TrackerArrows.Values.DestroyAll();
                            trackerRole.TrackerArrows.Clear();
                            Object.Destroy(trackerRole.UsesText);
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Lookout))
                        {
                            var loRole = Role.GetRole<Lookout>(PlayerControl.LocalPlayer);
                            Object.Destroy(loRole.UsesText);
                            seenPlayers = loRole.Watching;
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Aurial))
                        {
                            var aurialRole = Role.GetRole<Aurial>(PlayerControl.LocalPlayer);
                            aurialRole.SenseArrows.Values.DestroyAll();
                            aurialRole.SenseArrows.Clear();
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Mystic))
                        {
                            var mysticRole = Role.GetRole<Mystic>(PlayerControl.LocalPlayer);
                            mysticRole.BodyArrows.Values.DestroyAll();
                            mysticRole.BodyArrows.Clear();
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Transporter))
                        {
                            var transporterRole = Role.GetRole<Transporter>(PlayerControl.LocalPlayer);
                            Object.Destroy(transporterRole.UsesText);
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Veteran))
                        {
                            var veteranRole = Role.GetRole<Veteran>(PlayerControl.LocalPlayer);
                            Object.Destroy(veteranRole.UsesText);
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Trapper))
                        {
                            var trapperRole = Role.GetRole<Trapper>(PlayerControl.LocalPlayer);
                            Object.Destroy(trapperRole.UsesText);
                            trapperRole.traps.ClearTraps();
                            trappedPlayers = trapperRole.trappedPlayers;
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Oracle))
                        {
                            var oracleRole = Role.GetRole<Oracle>(PlayerControl.LocalPlayer);
                            oracleRole.ClosestPlayer = null;
                            oracleRole.ClosestBlessedPlayer = null;
                            oracleRole.BlessButton.SetTarget(null);
                            oracleRole.BlessButton.gameObject.SetActive(false);
                            confessingPlayer = oracleRole.Confessor;
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Warden))
                        {
                            var wardenRole = Role.GetRole<Warden>(PlayerControl.LocalPlayer);
                            wardenRole.ClosestPlayer = null;
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Deputy))
                        {
                            var deputyRole = Role.GetRole<Deputy>(PlayerControl.LocalPlayer);
                            deputyRole.ClosestPlayer = null;
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Detective))
                        {
                            var detecRole = Role.GetRole<Detective>(PlayerControl.LocalPlayer);
                            detecRole.ClosestPlayer = null;
                            detecRole.ExamineButton.gameObject.SetActive(false);
                            foreach (GameObject scene in detecRole.CrimeScenes)
                            {
                                UnityEngine.Object.Destroy(scene);
                            }
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Hunter))
                        {
                            var hunterRole = Role.GetRole<Hunter>(PlayerControl.LocalPlayer);
                            Object.Destroy(hunterRole.UsesText);
                            hunterRole.ClosestPlayer = null;
                            hunterRole.ClosestStalkPlayer = null;
                            hunterRole.StalkButton.SetTarget(null);
                            hunterRole.StalkButton.gameObject.SetActive(false);
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Cleric))
                        {
                            var clericRole = Role.GetRole<Cleric>(PlayerControl.LocalPlayer);
                            clericRole.ClosestPlayer = null;
                            clericRole.CleanseButton.SetTarget(null);
                            clericRole.CleanseButton.gameObject.SetActive(false);
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Politician))
                        {
                            var politicianRole = Role.GetRole<Politician>(PlayerControl.LocalPlayer);
                            politicianRole.ClosestPlayer = null;
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Jailor))
                        {
                            var jailorRole = Role.GetRole<Jailor>(PlayerControl.LocalPlayer);
                            jailorRole.ClosestPlayer = null;
                        }

                        if (PlayerControl.LocalPlayer.Is(RoleEnum.Plumber))
                        {
                            var plumberRole = Role.GetRole<Plumber>(PlayerControl.LocalPlayer);
                            plumberRole.Vent = null;
                            Object.Destroy(plumberRole.UsesText);

                            try
                            {
                                HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
                            }
                            catch { }
                        }

                        try
                        {
                            HudManager.Instance.KillButton.gameObject.SetActive(false);
                        }
                        catch { }
                    }

                    if (imi.Is(RoleEnum.Warden))
                    {
                        var warden = Role.GetRole<Warden>(imi);
                        if (warden.Fortified != null) ShowShield.ResetVisor(warden.Fortified, warden.Player);
                    }

                    if (imi.Is(RoleEnum.Medic))
                    {
                        var medic = Role.GetRole<Medic>(imi);
                        if (medic.ShieldedPlayer != null) ShowShield.ResetVisor(medic.ShieldedPlayer, medic.Player);
                    }

                    if (imi.Is(RoleEnum.Cleric))
                    {
                        var cleric = Role.GetRole<Cleric>(imi);
                        if (cleric.Barriered != null) cleric.UnBarrier();
                    }

                    if (imi.Is(RoleEnum.Medium))
                    {
                        var medRole = Role.GetRole<Medium>(imi);
                        medRole.MediatedPlayers.Values.DestroyAll();
                        medRole.MediatedPlayers.Clear();
                    }

                    if (imi.Is(RoleEnum.Snitch))
                    {
                        var snitchRole = Role.GetRole<Snitch>(imi);
                        snitchRole.SnitchArrows.Values.DestroyAll();
                        snitchRole.SnitchArrows.Clear();
                        snitchRole.ImpArrows.DestroyAll();
                        snitchRole.ImpArrows.Clear();
                    }

                    if (imi.Is(RoleEnum.Jailor))
                    {
                        var jailorRole = Role.GetRole<Jailor>(imi);
                        jailedPlayer = jailorRole.Jailed;
                    }

                    var role = Role.GetRole(imi);
                    var killsList = (role.Kills, role.CorrectKills, role.IncorrectKills, role.CorrectAssassinKills, role.IncorrectAssassinKills);
                    Role.RoleDictionary.Remove(imi.PlayerId);
                    var imitator = new Imitator(imi);
                    imitator.trappedPlayers = trappedPlayers;
                    imitator.confessingPlayer = confessingPlayer;
                    imitator.watchedPlayers = seenPlayers;
                    imitator.jailedPlayer = jailedPlayer;
                    var newRole = Role.GetRole(imi);
                    newRole.RemoveFromRoleHistory(newRole.RoleType);
                    newRole.Kills = killsList.Kills;
                    newRole.CorrectKills = killsList.CorrectKills;
                    newRole.IncorrectKills = killsList.IncorrectKills;
                    newRole.CorrectAssassinKills = killsList.CorrectAssassinKills;
                    newRole.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                    Role.GetRole<Imitator>(imi).ImitatePlayer = null;
                }
            }
            StartImitate.ImitatingPlayers.Clear();

            AddButtonDeputy.AddDepButtons(__instance);
            AddButtonImitator.AddImitatorButtons(__instance);
            TempJail.AddTempJail(__instance);
            AddJailButtons.AddJailorButtons(__instance);
            AddRevealButton.AddMayorButtons(__instance);
            AddRevealButtonPolitician.AddPoliticianButtons(__instance);
            AddProsecute.AddProsecuteButton(__instance);
            AddButton.AddSwapperButtons(__instance);
            AddButtonVigi.AddVigilanteButtons(__instance);
            BlackmailMeetingUpdate.MeetingHudStart.AddBlackmail(__instance);
            AddHysteriaButton.AddHypnoButtons(__instance);
            AddButtonAssassin.AddAssassinButtons(__instance);
            AddButtonDoom.AddDoomsayerButtons(__instance);
            return;
        }

        public static void CleansePlayer(byte id, List<EffectType> effects)
        {
            var player = Utils.PlayerById(id);
            if (effects.Contains(EffectType.Blackmail))
            {
                foreach (var bmer in Role.GetRoles(RoleEnum.Blackmailer))
                {
                    var bmerRole = (Blackmailer)bmer;
                    if (bmerRole.Blackmailed == player) bmerRole.Blackmailed = null;
                }
            }
            if (effects.Contains(EffectType.Hypnosis))
            {
                foreach (var hypno in Role.GetRoles(RoleEnum.Hypnotist))
                {
                    var hypnoRole = (Hypnotist)hypno;
                    if (hypnoRole.HypnotisedPlayers.Contains(id))
                    {
                        hypnoRole.HypnotisedPlayers.Remove(id);
                        if (PlayerControl.LocalPlayer == player && hypnoRole.HysteriaActive) hypnoRole.UnHysteria();
                    }
                }
            }
            if (effects.Contains(EffectType.Douse))
            {
                foreach (var arso in Role.GetRoles(RoleEnum.Arsonist))
                {
                    var arsoRole = (Arsonist)arso;
                    if (arsoRole.DousedPlayers.Contains(id)) arsoRole.DousedPlayers.Remove(id);
                }
            }
            if (effects.Contains(EffectType.Infect))
            {
                foreach (var pb in Role.GetRoles(RoleEnum.Plaguebearer))
                {
                    var pbRole = (Plaguebearer)pb;
                    if (pbRole.InfectedPlayers.Contains(id) && pbRole.Player != Utils.PlayerById(id)) pbRole.InfectedPlayers.Remove(id);
                }
            }
        }
    }
}