using System;
using System.Linq;
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
        
        [Command("rand", "Picks a random item from a list")]
        public static string Random(string words)
        {
            char[] possibleSplits = { ';', ',', '/', ' ' };

            string[] choices = possibleSplits
                                   .Where(c => words.Contains(c.ToString()))
                                   .Select(c => words.Split(c))
                                   .FirstOrDefault()
                ?? throw new Exception("Could not find choice separator.");

            return choices[new Random().Next(0, choices.Length)].Trim();
        }
    }
}
