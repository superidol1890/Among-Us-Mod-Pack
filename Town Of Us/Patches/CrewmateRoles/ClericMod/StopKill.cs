using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.ClericMod
{
    public class StopAttack
    {
        public static void NotifyCleric(byte clericId, bool showAttack = true)
        {
            if (!CustomGameOptions.ClericAttackNotification) return;
            if (showAttack) Coroutines.Start(Utils.FlashCoroutine(Color.blue));
            Utils.Rpc(CustomRPC.Barrier, clericId, (byte)2);
        }
    }
}