using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace HoLLy.DiscordBot.Commands
{
    internal class MethodCommand : Command
    {
        public override string Usage => Verb + _types?.Select(y => $" <{y.Name}>").SafeAggregate();

        private readonly Type[] _types;
        private readonly bool _endsOnVarLength;
        private readonly MethodInfo _method;

        public MethodCommand(string verb, string description, int? minPermission, MethodInfo method) : base(verb, description, minPermission ?? 0)
        {
            _method = method;

            Debug.Assert(_method != null);

            if (!_method.IsStatic)
                throw new ArgumentException($"Tried to create a {nameof(MethodCommand)} using a non-static method.", nameof(_method));

            _types = _method.GetParameters().Select(x => x.ParameterType).ToArray();
            _endsOnVarLength = _types.LastOrDefault() == typeof(string) || _types.LastOrDefault()?.IsArray == true;

            if (_types.Count(x => x == typeof(string)) >= 2)
                throw new NotSupportedException("Found more than 2 strings in this command.");
        }

        public override bool MatchesArguments(string arguments)
        {
            if (_types.Length == 0)
                return string.IsNullOrWhiteSpace(arguments);

            int argCount = arguments.Split(' ').Length;

            return _endsOnVarLength
                ? argCount >= _types.Length || _types.Length == 1
                : argCount == _types.Length;
        }

        public override object Invoke(string arguments) => _method.Invoke(null, ParseParameters(arguments));

        private object[] ParseParameters(string args)
        {
            if (_types.Length == 0)
                return null;

            if (_types.Length == 1) {
                if (!ParseParameter(_types[0], args, out object obj))
                    throw new ArgumentException($"Failed to parse argument! (type={_types[0]}) :(");
                return new[] { obj };
            }

            string[] split = args.Split(' ');
            if (!_endsOnVarLength && split.Length != _types.Length)
                throw new Exception($"Argument count mismatch! (Expected {_types.Length}, got {split.Length})");

            int paramCount = _types.Length;
            var parameters = new object[paramCount];
            if (_endsOnVarLength)
                paramCount--;   // parse 1 parameter less, keep the rest for the varlen param

            // parsing only the normal parameters
            for (int i = 0; i < paramCount; i++)
                if (!ParseParameter(_types[i], split[i], out parameters[i]))
                    throw new ArgumentException($"Failed to parse argument {i + 1} :(");

            // parsing varlen param, if needed
            if (_endsOnVarLength) {
                string finalArg = string.Join(" ", split.Skip(paramCount));
                if (!ParseParameter(_types[paramCount], finalArg, out parameters[paramCount]))
                    throw new ArgumentException($"Failed to parse variable length argument! (type={_types[paramCount]}) :(");
            }

            return parameters;
        }

        private static bool ParseParameter(Type t, string str, out object outParam)
        {
            bool success;

            if (t.IsArray) {
                Type elemType = t.GetElementType() ?? throw new Exception($"Array type {t} has null element type");

                string[] split = str.Split(' ');

                Array arr = Array.CreateInstance(elemType, split.Length);
                outParam = arr;

                for (int i = 0; i < split.Length; i++) {
                    if (!ParseParameter(elemType, split[i], out object temp))
                        return false;
                    arr.SetValue(temp, i);
                }

                return true;
            }

            switch (t.FullName) {
                case "System.Int32":
                    success = Int32.TryParse(str, out int i32);
                    outParam = i32;
                    break;
                case "System.String":
                    success = true;
                    outParam = str;
                    break;
                default:
                    throw new NotSupportedException($"Type {t} is cannot be parsed (yet?) and is not supported.");
            }

            return success;
        }
    }
}
