using System.Diagnostics;
using System.Linq;
using Discord.WebSocket;

namespace HoLLy.DiscordBot.Permissions.Conditions.Users
{
    [DebuggerDisplay("ROLE[{_name.ToString()}]")]
    internal class UserRoleCondition : IUserCondition
    {
        private readonly string _name;

        public UserRoleCondition(string name)
        {
            _name = name;
        }

        public bool Match(SocketUser user, ISocketMessageChannel c)
        {
            // We need this user to be in a server
            if (user is SocketGuildUser gu)
                return gu.Roles.Any(x => x.Name == _name);

            return false;
        }
    }
}
