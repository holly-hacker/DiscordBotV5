using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoLLy.DiscordBot.Commands {
    internal class HelpCommand : Command
    {
        private readonly IReadOnlyList<Command> _commands;

        public HelpCommand(IReadOnlyList<Command> commands) : base("help", "Displays some help")
        {
            _commands = commands;
        }

        public override string Usage => Verb;

        public override bool MatchesArguments(string arguments) => true;

        public override object Invoke(string arguments)
        {
            // Ignoring arguments for now
            // TODO: show help-specific info if argument is specified (also make sure to update Match when implementing that)

            // Get a list of commands (with params)
            List<string> usages = _commands.Select(x => x.Usage + ":").ToList();
            int longestUsage = usages.Max(x => x.Length);

            // Build the help list
            var sb = new StringBuilder();
            sb.AppendLine("```http");
            for (int i = 0; i < usages.Count; i++)
                sb.AppendLine($"{usages[i].PadRight(longestUsage)} {_commands[i].Description ?? "<no description>"}");
            sb.Append("```");

            return sb.ToString();
        }
    }
}