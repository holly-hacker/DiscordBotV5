using System;
using System.Linq;
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

        [Command("add", "Adds 3(!!) things")]
        public static int Add(int x, int y, int z) => x + y + z;

        [Command("Repeat", "Repeats a string")]
        public static string Repeat(int count, string s) => new string(Enumerable.Repeat(s.ToCharArray(), count).SelectMany(x => x).ToArray());

        [Command("rand", "Picks a random word from a list")]
        public static string Random(string[] words) => words[new Random().Next(0, words.Length)];

        [Command("nothing")]
        public static void Dummy() {}
    }
}
