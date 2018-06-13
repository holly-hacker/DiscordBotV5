using System.Diagnostics;
using Discord.WebSocket;

namespace HoLLy.DiscordBot.Permissions.Conditions.Users
{
    [DebuggerDisplay("DEFAULTUSER")]
    internal class DefaultUserCondition : IUserCondition
    {
        public bool Match(SocketUser user, ISocketMessageChannel c) => true;
    }
}
