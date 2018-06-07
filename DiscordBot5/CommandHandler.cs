using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using NCalc;

namespace HoLLy.DiscordBot
{
    internal class CommandHandler
    {
        private readonly string _prefix;

        public CommandHandler(string prefix)
        {
            _prefix = prefix;
        }

        public async Task Handle(SocketMessage msg)
        {
            string content = msg.Content;

            string resp;
            if (content.StartsWith(_prefix)) {
                string command = content.Substring(_prefix.Length).TrimStart();
                resp = await HandleCommand(msg, command);
            } else {
                var sb = new StringBuilder();
                foreach (var pair in ExtractCommands(content))
                    sb.AppendLine($"`{pair.Key}` - {await HandleCommand(msg, pair.Value)}");

                resp = sb.ToString();
            }

            await msg.Channel.SendMessageAsync(resp);
        }

        private async Task<string> HandleCommand(SocketMessage msg, string cmd)
        {
            // For now, have simple command parsing that ignores if the user is trusted.
            var sidx = cmd.IndexOf(" ");
            if (sidx == -1) {
                // No parameters
            } else {
                var verb = cmd.Substring(0, sidx);
                var param = cmd.Substring(sidx + 1);
                switch (verb.ToLower()) {
                    case "calc":
                        return new Expression(param).Evaluate().ToString() ?? "null";
                }
            }

            return "Pong: " + cmd;
        }

        private IEnumerable<KeyValuePair<string, string>> ExtractCommands(string src)
        {
            int unnamedCount = 0;
            foreach (Match match in Regex.Matches(src, $@"{{{Regex.Escape(_prefix)}(?:(?<name>[^|}}]+) ?\| ?)?(?<cmd>[^}}]+)}}"))
                yield return new KeyValuePair<string, string>(
                    match.Groups["name"].Success ? match.Groups["name"].Value : "cmd" + ++unnamedCount,
                    match.Groups["cmd"].Value.Trim());
        }
    }
}
