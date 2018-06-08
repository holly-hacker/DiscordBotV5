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
        public string Usage => Verb + Types?.Select(y => $" <{y.Name}>").SafeAggregate();
        private MethodInfo _method;
        private bool EndsOnString => Types?.Last() == typeof(string);

        public Command(string verb, string description, MethodInfo method)
        {
            Verb = verb.ToLowerInvariant();
            Description = description;
            _method = method;

            if (_method != null) {
                Debug.Assert(_method.IsStatic);

                // Do some checks against strings. I don't like strings :(
                if (Types.Count(x => x == typeof(string)) >= 2)
                    throw new NotSupportedException("Found more than 2 strings in this command.");
            }
        }

        public bool Matches(string verb, string arguments)
        {
            // Easiest check, make sure the verbs match
            if (verb != Verb) return false;

            // If we don't have a method (and thus no types), make sure the arguments aren't specified
            if (Types == null || Types.Length == 0)
                return string.IsNullOrWhiteSpace(arguments);

            int argCount = arguments.Split(' ').Length;

            if (!EndsOnString) {
                // If we do NOT end on a string, then check if the amount of args is equal to the expected
                return argCount == Types.Length;
            } else {
                // Ending on a string, usually not good. Do some special checks
                if (Types.Length == 1)
                    return true;    // The entire argument is a single string
                else
                    return argCount >= Types.Length;
            }

        }

        public virtual object Invoke(string arguments) => _method.Invoke(null, ParseParameters(arguments));

        private object[] ParseParameters(string args)
        {
            // Special case: no parameters expected
            if (Types == null || Types.Length == 0)
                return null;

            // Special case: accepting only a string
            if (Types.Length == 1 && Types[0] == typeof(string))
                return new object[] { args };

            // Detect if the last parameter is a string, we need some special handling. then 

            // Split the argument by space, assuming the last argument isn't a string
            string[] splittedArgs = args.Split(' ');
            if (!EndsOnString && splittedArgs.Length != Types.Length)
                throw new Exception($"Argument count mismatch! (Expected {Types.Length}, got {splittedArgs.Length})");

            // Get the length of the normal parameters (meaning not strings).
            // If there is a string at the end, then we stop the splitting one parameter early and take the rest as the string parameter.
            int paramCount = splittedArgs.Length;
            var parameters = new object[paramCount];
            if (EndsOnString)
                paramCount--;   // Need to keep in mind that this is reduced by 1 if we end on a string!

            // Everything should be good now, let's parse the parameters
            for (int i = 0; i < paramCount; i++)
                if (!ParseParameter(Types[i], splittedArgs[i], out parameters[i]))
                    throw new ArgumentException($"Failed to parse argument {i + 1} :(");

            // If the last parameter is a string, get that last string now
            if (EndsOnString) {
                Debug.Assert(paramCount == splittedArgs.Length - 1);
                parameters[paramCount] = string.Join(" ", splittedArgs.Skip(paramCount));
            }

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
