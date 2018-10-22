using System;

namespace HoLLy.DiscordBot.Commands
{
    internal static class ParameterParser
    {
        public static bool IsVarLen(Type t) => t == typeof(string) || t.IsArray;

        // TODO: move parameter parsing here
    }
}
