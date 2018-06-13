using Discord.WebSocket;

namespace HoLLy.DiscordBot.Permissions.Conditions.Servers
{
    internal interface IServerCondition
    {
        bool Match(ISocketMessageChannel c);
    }
}
