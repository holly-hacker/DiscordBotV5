using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HoLLy.DiscordBot.Commands.DependencyInjection;

namespace HoLLy.DiscordBot.Commands
{
    internal class MethodCommand : Command
    {
        public override string Usage => Verb + _cmdParams?.Select(y => $" <{y.Name}>").SafeAggregate();

        private readonly Type[] _cmdParams;
        private readonly DependencyContainer _dep;
        private readonly bool _endsOnVarLength;
        private readonly MethodInfo _method;
        private readonly int _paramCount;

        internal MethodCommand(CommandAttribute cmdAttr, MethodInfo method, DependencyContainer dep) : base(cmdAttr.Command, cmdAttr.Description, cmdAttr.MinPermission ?? 0)
        {
            _method = method;
            _dep = dep;

            Debug.Assert(_method != null);

            if (!_method.IsStatic)
                throw new ArgumentException($"Tried to create a {nameof(MethodCommand)} using a non-static method.", nameof(_method));

            _cmdParams = _method.GetParameters().Where(x => x.CustomAttributes.All(y => y.AttributeType != typeof(DIAttribute))).Select(x => x.ParameterType).ToArray();
            _paramCount = _cmdParams.Length;
            _endsOnVarLength = _paramCount > 0 && ParameterParser.IsVarLen(_cmdParams.Last());

            if (_cmdParams.Count(ParameterParser.IsVarLen) > 1)
                throw new NotSupportedException("Found more than 2 strings in this command.");
        }

        protected override bool MatchesArguments(string arguments)
        {
            if (_paramCount == 0)
                return string.IsNullOrWhiteSpace(arguments);

            int argCount = arguments.Split(' ').Length;

            return _endsOnVarLength
                ? argCount >= _paramCount || _paramCount == 1
                : argCount == _paramCount;
        }

        public override object Invoke(string arguments) => _method.Invoke(null, ParseParameters(arguments));

        private object[] ParseParameters(string args)
        {
            var implParams = _method.GetParameters();
            var retParams = new object[implParams.Length];
            string[] split = (args ?? string.Empty).Split(' ');
            int argIdx = 0;
            bool cannotParseMore = false;

            for (int i = 0; i < implParams.Length; i++) {
                ParameterInfo ip = implParams[i];
                Type ipType = ip.ParameterType;
                if (ip.CustomAttributes.Any(x => x.AttributeType == typeof(DIAttribute))) {
                    retParams[i] = _dep.Get(ipType);
                    // TODO: error checking, eg. when not cached
                } else {
                    if (cannotParseMore)
                        throw new Exception("Continued parsing after being told not to (multiple varlens?)");

                    if (ParameterParser.IsVarLen(ipType)) { // this check relies on a check on multiple varlens being done before (see ctor)
                        if (!ParseParameter(ipType, string.Join(" ", split.Skip(argIdx++)), out retParams[i]))
                            throw new ArgumentException($"Failed to parse varlen argument {argIdx} :(");
                        cannotParseMore = true;
                    }
                    else if (!ParseParameter(ipType, split[argIdx++], out retParams[i]))
                        throw new ArgumentException($"Failed to parse argument {argIdx} :(");
                }
            }

            return retParams;
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
