using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HoLLy.DiscordBot.Commands;

namespace HoLLy.DiscordBot
{
    internal static class Program
    {
        private const string TokenEnvName = "DISCORD_BOT_SECRET";
        private const string Prefix = "h:";

        private static CommandHandler _cmd;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!\n");

            _cmd = new CommandHandler(Prefix);
            _cmd.InstallCommands();

            var client = new DiscordSocketClient(); 

            // Artificial scope to make sure the token doesn't leak by accident :)
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
            await _cmd.Handle(arg);
        }

        private static Task ClientOnLog(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}
