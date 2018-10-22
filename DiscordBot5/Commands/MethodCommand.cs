using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace HoLLy.DiscordBot.Commands
{
    internal class MethodCommand : CommandBase
    {
        public override string Usage => Verb + Types?.Select(y => $" <{y.Name}>").SafeAggregate();

        private Type[] Types => _method.GetParameters().Select(x => x.ParameterType).ToArray();
        private bool EndsOnVarLength => Types.Last() == typeof(string) || Types?.Last().IsArray == true;

        private readonly MethodInfo _method;

        public MethodCommand(string verb, string description, int? minPermission, MethodInfo method) : base(verb, description, minPermission ?? 0)
        {
            _method = method;

            Debug.Assert(_method != null);
            
            if (!_method.IsStatic)
                throw new ArgumentException($"Tried to create a {nameof(MethodCommand)} using a non-static method.", nameof(_method));

            // Do some checks against strings. I don't like strings :(
            if (Types.Count(x => x == typeof(string)) >= 2)
                throw new NotSupportedException("Found more than 2 strings in this command.");
        }
        
        public override bool MatchesArguments(string arguments)
        {
            // If we don't have a method (and thus no types), make sure the arguments aren't specified
            if (Types == null || Types.Length == 0)
                return string.IsNullOrWhiteSpace(arguments);

            int argCount = arguments.Split(' ').Length;

            if (!EndsOnVarLength) {
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

        public override object Invoke(string arguments) => _method.Invoke(null, ParseParameters(arguments));

        private object[] ParseParameters(string args)
        {
            // Special case: no parameters expected
            if (Types == null || Types.Length == 0)
                return null;

            // Special case: accepting only a variable length type
            if (Types.Length == 1 && EndsOnVarLength)
                if (ParseParameter(Types[0], args, out object obj)) {
                    return new[] { obj };
                } else throw new ArgumentException($"Failed to parse variable length argument! (type={Types[0]}) :(");
            

            // Detect if the last parameter is a string, we need some special handling then 

            // Split the argument by space, assuming the last argument isn't a string
            string[] splittedArgs = args.Split(' ');
            if (!EndsOnVarLength && splittedArgs.Length != Types.Length)
                throw new Exception($"Argument count mismatch! (Expected {Types.Length}, got {splittedArgs.Length})");

            // Get the length of the normal parameters (meaning not variable lengths).
            // If there is a variable length at the end, then we stop the splitting one parameter early and take the rest as the parameter.
            int paramCount = Types.Length;
            var parameters = new object[paramCount];
            if (EndsOnVarLength)
                paramCount--;   // Need to keep in mind that this is reduced by 1 if we end on a variable length!

            // Everything should be good now, let's parse the parameters
            for (int i = 0; i < paramCount; i++)
                if (!ParseParameter(Types[i], splittedArgs[i], out parameters[i]))
                    throw new ArgumentException($"Failed to parse argument {i + 1} :(");

            // If the last parameter is of variable length, get that now
            if (EndsOnVarLength) {
                // Take the last (joined left args) and parse it
                var finalArg = string.Join(" ", splittedArgs.Skip(paramCount));
                if (!ParseParameter(Types[paramCount], finalArg, out parameters[paramCount]))
                    throw new ArgumentException($"Failed to parse last (variable length) argument! (type={Types[paramCount]}) :(");
            }

            return parameters;
        }

        private static bool ParseParameter(Type t, string str, out object o)
        {
            bool ret;

            // Check if array
            if (t.IsArray) {
                // Split the remaining string
                var splitted = str.Split(' ');
                
                Debug.Assert(t.GetElementType() != null);
                Array arr = Array.CreateInstance(t.GetElementType(), splitted.Length);    // This has to be an array of the correct type!
                o = arr;
                
                // Try to parse all the parameters, and then do some recursive magic!
                for (int i = 0; i < splitted.Length; i++) {
                    bool succ = ParseParameter(t.GetElementType(), splitted[i], out object temp);
                    arr.SetValue(temp, i);
                    if (!succ)
                        return false;
                }

                return true;
            }

            switch (t.FullName) {
                case "System.Int32":
                    ret = Int32.TryParse(str, out int i32);
                    o = i32;
                    break;
                case "System.String":
                    ret = true;
                    o = str;
                    break;
                default:
                    o = null;
                    return false;
            }

            return ret;
        }
    }
}
