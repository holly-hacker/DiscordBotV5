using System;
using NCalc;

namespace HoLLy.DiscordBot.Commands
{
    public static class UtilityCommands
    {
        [Command("calc", "Calculates stuff")]
        public static string Calc(string input)
        {
            try {
                return new Expression(input).Evaluate().ToString();
            } catch (Exception e) {
                return $"Error during evaluation: `{e.Message}`";
            }
        }

        [Command("add", "Adds 2 things")]
        public static int Add(int x, int y) => x + y;
    }
}
