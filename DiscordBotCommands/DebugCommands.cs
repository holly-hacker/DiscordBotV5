#if DEBUG
using System.Linq;
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
        public static string DITest([DI] PermissionManager perms, [DI] CommandHandler cmd)
        {
            return $"{cmd.Commands.Count} commands loaded";
        }
    }
}
#endif