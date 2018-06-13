using Discord.WebSocket;

namespace HoLLy.DiscordBot.Permissions.Conditions.Users
{
    internal interface IUserCondition
    {
        bool Match(SocketUser user, ISocketMessageChannel c);
    }
}
