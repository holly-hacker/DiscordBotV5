#if DEBUG
using System.Linq;
using Discord.WebSocket;
using HoLLy.DiscordBot.Commands.DependencyInjection;
using HoLLy.DiscordBot.Permissions;

namespace HoLLy.DiscordBot.Commands
{
    public static class DebugCommands
    {
        [Command("add", "Adds 2 things")]
        public static int Add(int x, int y) => x + y;

        [Command("add", "Adds 3(!!) things")]
        public static int Add(int x, int y, int z) => x + y + z;

        [Command("repeat", "Repeats a string")]
        public static string Repeat(int count, string s) => new string(Enumerable.Repeat(s.ToCharArray(), count).SelectMany(x => x).ToArray());

        [Command("nothing")]
        public static void Dummy() {}

        [Command(30, "permtest1", "Requires a permission level of at least 30")]
        public static string PermissionTest1() => "Permission test success!";

        [Command(130, "permtest2", "Requires a permission level of at least 130")]
        public static string PermissionTest2() => "Permission test success!";

        [Command("ditest")]
        // parameters are in weird order on purpose
        public static string DITest([DI] SocketMessage msg, string idk, [DI] CommandHandler cmd) => $"{cmd.Commands.Count} commands loaded. \n" +
                                                                                                    $"Message type is `{msg.GetType()}`. \n" +
                                                                                                    $"Passed parameter: `{idk}`";

        [Command("permtest", "Shows callers permission level in this context")]
        public static string PermissionTest([DI] SocketMessage msg, [DI] PermissionManager perms) => $"Your level is {perms.GetPermissionLevel(msg)}";

        [Command("echo", "Returns the entire source message")]
        public static string Echo([DI] SocketUserMessage msg) => msg.Resolve();

        [Command("who", "Returns self userinfo")]
        public static string SelfInfo([DI] DiscordSocketClient cl)
        {
            return $"Username: {cl.CurrentUser.Username}#{cl.CurrentUser.DiscriminatorValue}\n" +
                   $"User ID: {cl.CurrentUser.Id}\n" +
                   $"Avatar ID: {cl.CurrentUser.AvatarId}";
        }
    }
}
#endif