using System;
using System.Diagnostics;
using Discord.WebSocket;

namespace HoLLy.DiscordBot.Permissions.Conditions.Users
{
    [DebuggerDisplay("USERID[{_id.ToString()}]")]
    internal class UserIdCondition : IUserCondition
    {
        private readonly ulong _id;

        public UserIdCondition(ulong id)
        {
            _id = id;
        }

        public bool Match(SocketUser user, ISocketMessageChannel c)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return user.Id == _id;
        }
    }
}
