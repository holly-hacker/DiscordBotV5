using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HoLLy.DiscordBot.Commands
{
    internal class Command
    {
        public string Verb;
        public string Description;
        public Type[] Types => _method?.GetParameters().Select(x => x.ParameterType).ToArray();
        private MethodInfo _method;

        public Command(string verb, string description, MethodInfo method)
        {
            Verb = verb.ToLowerInvariant();
            Description = description;
            _method = method;

            if (_method != null)
                Debug.Assert(_method.IsStatic);
        }

        public bool Matches(string verb, string arguments)
        {
            // For now, only check the verb
            // TODO: Check if arguments match
            return verb == Verb;
        }

        public virtual object Invoke(string arguments)
        {
            return _method.Invoke(null, ParseParameters(arguments));
        }

        private object[] ParseParameters(string args)
        {
            // Special case: accepting only a string
            if (Types.Length == 1 && Types[0] == typeof(string))
                return new object[] { args };

            // Do some checks against strings. I don't like strings :(
            if (Types.Last() == typeof(string))
                throw new NotImplementedException("Last type of command params is a string, this is not supported");
            if (Types.Any(x => x == typeof(string)))
                throw new NotImplementedException("You know what, strings in general in command parameters aren't supported.");

            // Split the argument by space, assuming the last argument isn't a string
            string[] splittedArgs = args.Split(' ');
            if (splittedArgs.Length != Types.Length)
                throw new Exception($"Argument count mismatch! (Expected {Types.Length}, got {splittedArgs.Length})");

            // Everything should be good now, let's parse the parameters
            var parameters = new object[splittedArgs.Length];
            for (int i = 0; i < splittedArgs.Length; i++)
                if (!ParseParameter(Types[i], splittedArgs[i], out parameters[i]))
                    throw new ArgumentException($"Failed to parse argument {i + 1} :(");
            return parameters;
        }

        private static bool ParseParameter(Type t, string str, out object o)
        {
            bool ret;
            switch (t.FullName) {
                case "System.Int32":
                    ret = Int32.TryParse(str, out int i32);
                    o = i32;
                    break;
                default:
                    o = null;
                    return false;
            }

            return ret;
        }
    }

    internal class HelpCommand : Command
    {
        private readonly IReadOnlyList<Command> _commands;

        public HelpCommand(IReadOnlyList<Command> commands) : base("help", "Displays some help", null)
        {
            _commands = commands;
        }

        public override object Invoke(string arguments)
        {
            // Ignoring arguments for now
            // TODO: show help-specific info if argument is specified

            // Get a list of commands (with params)
            List<string> usages = _commands.Select(x => x.Verb + x.Types?.Select(y => $" <{y.Name}>").Aggregate((i, j) => i + j) + ":").ToList();
            int longestUsage = usages.Max(x => x.Length);

            // Build the help list
            var sb = new StringBuilder();
            sb.AppendLine("```http");
            for (int i = 0; i < usages.Count; i++)
                sb.AppendLine($"{usages[i].PadRight(longestUsage)} {_commands[i].Description}");
            sb.Append("```");

            return sb.ToString();
        }
    }
}
