using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HoLLy.DiscordBot.Commands;
using HoLLy.DiscordBot.Commands.DependencyInjection;
using HoLLy.DiscordBot.Permissions;

namespace HoLLy.DiscordBot
{
    internal static class Program
    {
        private const string TokenEnvName = "DISCORD_BOT_SECRET";
        private const string PermissionFile = "permissions.txt";

        #if DEBUG
        private const string Prefix = "ht:";
        #else
        private const string Prefix = "h:";
        #endif

        private static CommandHandler _cmd;
        private static PermissionManager _perm;
        private static DependencyContainer _dep;

        private static async Task Main()
        {
            Console.WriteLine("Hello World!\n");

            if (!File.Exists(PermissionFile)) {
                Console.WriteLine($"Please create a permissions file named {PermissionFile}!");
                Console.Read();
                return;
            }

            var client = new DiscordSocketClient();
            _dep = new DependencyContainer();
            _perm = new PermissionManager();
            _cmd = new CommandHandler(Prefix, _perm, _dep);

            Console.WriteLine("Caching dependencies");
            _dep.Cache(_perm);
            _dep.Cache(_cmd);
            _dep.Cache(client);

            Console.WriteLine("Reading permissions...");
            _perm.Read(PermissionFile);

            Console.WriteLine("Installing commands...");
            _cmd.InstallCommands();

            // Artificial scope to make sure the token doesn't leak by accident during debugging :)
            {
                string token = Environment.GetEnvironmentVariable(TokenEnvName);
                if (string.IsNullOrWhiteSpace(token)) {
                    Console.WriteLine($"No token in {TokenEnvName}.");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("Logging in...");
                await client.LoginAsync(TokenType.Bot, token);
            }

            Console.WriteLine("Starting...");
            await client.StartAsync();
            Console.WriteLine("Logged in.");

            // Setting up events
            await client.SetGameAsync("dead");
            client.Log += ClientOnLog;
            client.MessageReceived += ClientOnMessageReceived;

            await Task.Delay(-1);
        }

        private static async Task ClientOnMessageReceived(SocketMessage arg)
        {
            await _cmd.HandleMessage(arg);
        }

        private static Task ClientOnLog(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}
