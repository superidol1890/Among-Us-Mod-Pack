using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Server.Interfaces;
using Lotus.Options;
using VentLib.Utilities.Extensions;

namespace Lotus.Server.Handlers;

internal class PostMeetingHandlers
{
    public static IPostMeetingHandler StandardHandler = new Standard();
    public static IPostMeetingHandler ProtectionPatchedHandler = new ProtectionPatched();

    private class Standard : IPostMeetingHandler
    {
        public void PostMeetingSetup()
        {

        }
    }

    private class ProtectionPatched : IPostMeetingHandler
    {
        public void PostMeetingSetup()
        {
            bool randomSpawn = GeneralOptions.MayhemOptions.UseRandomSpawn;

            Players.GetPlayers().ForEach(p =>
            {
                p.RpcProtectPlayer(p, 0);
                if (randomSpawn) Game.RandomSpawn.Spawn(p);
            });
        }
    }
}