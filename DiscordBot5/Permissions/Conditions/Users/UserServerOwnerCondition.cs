using System.Diagnostics;
using Discord.WebSocket;

namespace HoLLy.DiscordBot.Permissions.Conditions.Users
{
    [DebuggerDisplay("SERVEROWNER")]
    internal class UserServerOwnerCondition : IUserCondition
    {
        public bool Match(SocketUser user, ISocketMessageChannel c)
        {
            if (c is SocketGuildChannel gc)
                return gc.Guild.OwnerId == user.Id;

            return false;
        }
    }
}
