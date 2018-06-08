using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace HoLLy.DiscordBot.Commands
{
    internal class CommandHandler
    {
        private const string AssemblyPrefix = "HoLLy.DiscordBot.Commands";

        private readonly string _prefix;
        private List<Command> _commands;

        public CommandHandler(string prefix)
        {
            _prefix = prefix;
        }

        public void InstallCommands()
        {
            _commands = new List<Command>();

            // Install the default help command
            _commands.Add(new HelpCommand(_commands));  // Command-ception

            // Look for commands in nearby DLL's
            foreach (var command in FindCommands()) {
                Console.WriteLine("Detected command " + command.Verb);
                _commands.Add(command);
            }
        }

        public async Task Handle(SocketMessage msg)
        {
            if (msg.Source != MessageSource.User) return;

            string content = msg.Content;

            string response;
            if (content.StartsWith(_prefix)) {
                // A normal command, triggered by a message starting with a prefix
                string command = content.Substring(_prefix.Length).TrimStart();
                response = await HandleCommand(msg, command);
            } else {
                // Check if this message contains any interpolated commands
                // This code lazily initialized sb, as a minor performance optimization
                StringBuilder sb = null;
                foreach (var pair in ExtractCommands(content))
                    (sb ?? (sb = new StringBuilder())).AppendLine($"`{pair.Key}` - {await HandleCommand(msg, pair.Value)}");

                response = sb?.ToString();
            }

            if (!string.IsNullOrEmpty(response))
                await msg.Channel.SendMessageAsync(response);
        }

        private async Task<string> HandleCommand(SocketMessage msg, string cmd)
        {
            // Do some basic parsing
            int spaceIdx = cmd.IndexOf(' ');
            string verb, args;
            if (spaceIdx == -1) {
                verb = cmd;
                args = null;
            }
            else {
                verb = cmd.Substring(0, spaceIdx);
                args = cmd.Substring(spaceIdx + 1);
            }

            // Look for any matching commands
            var matching = _commands.Where(x => x.Matches(verb, args)).ToList();
            if (matching.Count == 0) {
                // No matching commands found
                return "No matching command found!";
            } else if (matching.Count > 1) {
                // Too many commands found! This is probably not good.
                return "Too many matching commands found! Please contact the developer";
            } else {
                // We got a single command, invoke it.
                try {
                    return matching.Single().Invoke(args).ToString();
                } catch (Exception e) {
                    Console.WriteLine(e);
                    return $"An exception occured while executing this command: `{e.Message}`";
                }
            }
        }

        private IEnumerable<KeyValuePair<string, string>> ExtractCommands(string src)
        {
            int unnamedCount = 0;
            foreach (Match match in Regex.Matches(src, $@"{{{Regex.Escape(_prefix)}(?:(?<name>[^|}}]+) ?\| ?)?(?<cmd>[^}}]+)}}"))
                yield return new KeyValuePair<string, string>(
                    match.Groups["name"].Success ? match.Groups["name"].Value : "cmd" + ++unnamedCount,
                    match.Groups["cmd"].Value.Trim());
        }

        public static IEnumerable<Command> FindCommands()
        {
            return  from file   in Directory.GetFiles(Environment.CurrentDirectory, AssemblyPrefix + "*.dll") 
                from method in Assembly.LoadFile(file).ExportedTypes.SelectMany(x => x.GetMethods()) 
                from attr   in method.GetCustomAttributes<CommandAttribute>() 
                select new Command(attr.Command, attr.Description, method);
        }
    }
}
