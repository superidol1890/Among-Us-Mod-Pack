using System.Collections.Generic;
using HarmonyLib;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles.Interactions;
using Lotus.Utilities;
using Lotus.Options;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.Patches
{
    public class FallFromLadder
    {
        public static Dictionary<byte, Vector3> TargetLadderData;
        // TODO: FIX THIS LOL
        private static int Chance => GeneralOptions.GameplayOptions.LadderDeathChance;
        public static void Reset()
        {
            TargetLadderData = new();
        }
        public static void OnClimbLadder(PlayerPhysics player, Ladder source)
        {
            if (!GeneralOptions.GameplayOptions.EnableLadderDeath) return;
            var sourcePos = source.transform.position;
            var targetPos = source.Destination.transform.position;
            //降りているのかを検知
            if (sourcePos.y > targetPos.y)
            {
                int chance = UnityEngine.Random.RandomRangeInt(1, 101);
                if (chance <= Chance)
                {
                    TargetLadderData[player.myPlayer.PlayerId] = targetPos;
                }
            }
        }
        public static void FixedUpdate(PlayerControl player)
        {
            if (player.Data.Disconnected) return;
            if (TargetLadderData.ContainsKey(player.PlayerId))
            {
                if (Vector2.Distance(TargetLadderData[player.PlayerId], player.transform.position) < 0.5f)
                {
                    if (player.Data.IsDead) return;
                    //LateTaskを入れるため、先に死亡判定を入れておく
                    player.Data.IsDead = true;
                    Async.Schedule(() =>
                    {
                        Vector2 targetPos = (Vector2)TargetLadderData[player.PlayerId] + new Vector2(0.1f, 0f);
                        ushort num = (ushort)(NetHelpers.XRange.ReverseLerp(targetPos.x) * 65535f);
                        ushort num2 = (ushort)(NetHelpers.YRange.ReverseLerp(targetPos.y) * 65535f);

                        Utils.Teleport(player.NetTransform, new Vector2(num, num2));
                        player.InteractWith(player, new UnblockedInteraction(new FatalIntent(), player.PrimaryRole()));
                    }, 0.05f);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
    class LadderPatch
    {
        public static void Postfix(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
        {
            FallFromLadder.OnClimbLadder(__instance, source);
        }
    }
}