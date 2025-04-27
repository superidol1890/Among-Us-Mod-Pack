using System;
using HarmonyLib;
using Hazel;
using Reactor.Utilities;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.ClericMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Cleric);
            if (!flag) return true;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            var role = Role.GetRole<Cleric>(PlayerControl.LocalPlayer);
            if (!__instance.isActiveAndEnabled || __instance.isCoolingDown) return false;

            if (__instance == role.CleanseButton)
            {
                if (role.BarrierTimer() == 0)
                {
                    if (role.ClosestPlayer == null) return false;
                    var interact2 = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
                    if (interact2[4] == true)
                    {
                        var effects = role.FindNegativeEffects(role.ClosestPlayer);
                        if (effects.Count != 0)
                        {
                            Coroutines.Start(Utils.FlashCoroutine(role.Color));
                            if (role.CleansedPlayers.ContainsKey(role.ClosestPlayer.PlayerId))
                            {
                                foreach (var effect in effects)
                                {
                                    if (!role.CleansedPlayers[role.ClosestPlayer.PlayerId].Contains(effect))
                                    {
                                        role.CleansedPlayers[role.ClosestPlayer.PlayerId].Add(effect);
                                    }
                                }
                            }
                            else role.CleansedPlayers[role.ClosestPlayer.PlayerId] = effects;

                            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                                (byte)CustomRPC.Barrier, SendOption.Reliable, -1);
                            writer.Write((byte)PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)1);
                            writer.Write((byte)role.ClosestPlayer.PlayerId);
                            writer.Write((byte)effects.Count);
                            foreach (var effect in effects)
                            {
                                writer.Write((byte)effect);
                            }
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                    }
                    if (interact2[0] == true)
                    {
                        role.LastBarriered = DateTime.UtcNow;
                        return false;
                    }
                    else if (interact2[1] == true)
                    {
                        role.LastBarriered = DateTime.UtcNow;
                        role.LastBarriered.AddSeconds(CustomGameOptions.TempSaveCdReset - CustomGameOptions.BarrierCd);
                        return false;
                    }
                    else if (interact2[3] == true) return false;
                    return false;
                }
                else return false;
            }

            if (__instance != HudManager.Instance.KillButton) return true;
            if (role.ClosestPlayer == null) return false;
            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
            if (interact[4] == true)
            {
                role.Barriered = role.ClosestPlayer;
                role.TimeRemaining = CustomGameOptions.BarrierCd;
                Utils.Rpc(CustomRPC.Barrier, PlayerControl.LocalPlayer.PlayerId, (byte)0, role.Barriered.PlayerId);
            }
            if (interact[0] == true)
            {
                role.LastBarriered = DateTime.UtcNow;
                return false;
            }
            else if (interact[1] == true)
            {
                role.LastBarriered = DateTime.UtcNow;
                role.LastBarriered.AddSeconds(CustomGameOptions.TempSaveCdReset - CustomGameOptions.BarrierCd);
                return false;
            }
            else if (interact[3] == true) return false;
            return false;
        }
    }
}
