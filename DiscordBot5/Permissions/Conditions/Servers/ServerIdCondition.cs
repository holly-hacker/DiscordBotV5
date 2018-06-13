using System.Diagnostics;
using Discord.WebSocket;

namespace HoLLy.DiscordBot.Permissions.Conditions.Servers
{
    [DebuggerDisplay("SERVERID[{_id.ToString()}]")]
    internal class ServerIdCondition : IServerCondition
    {
        private readonly ulong _id;

        public ServerIdCondition(ulong id)
        {
            _id = id;
        }

        public bool Match(ISocketMessageChannel c) => c.Id == _id;
    }
}
