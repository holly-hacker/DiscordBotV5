using System.Diagnostics;
using Discord.WebSocket;

namespace HoLLy.DiscordBot.Permissions.Conditions.Servers
{
    [DebuggerDisplay("DEFAULTSERVER")]
    internal class DefaultServerCondition : IServerCondition
    {
        public bool Match(ISocketMessageChannel c) => true;
    }
}
