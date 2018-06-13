using System;
using NCalc;

namespace HoLLy.DiscordBot.Commands
{
    public class UtilityCommands
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
    }
}
