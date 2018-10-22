using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HoLLy.DiscordBot.Commands;
using HoLLy.DiscordBot.Permissions;

namespace HoLLy.DiscordBot
{
    internal static class Program
    {
        private const string TokenEnvName = "DISCORD_BOT_SECRET";
        private const string PermissionFile = "permissions.txt";
        private const string Prefix = "h:";

        private static CommandHandler _cmd;
        private static PermissionManager _perm;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!\n");

            if (!File.Exists(PermissionFile)) {
                Console.WriteLine($"Please create a permissions file named {PermissionFile}!");
                Console.Read();
                return;
            }

            Console.WriteLine("Reading permissions...");
            _perm = new PermissionManager();
            _perm.Read(PermissionFile);

            Console.WriteLine("Installing commands...");
            _cmd = new CommandHandler(Prefix, _perm);
            _cmd.InstallCommands();

            var client = new DiscordSocketClient(); 

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
