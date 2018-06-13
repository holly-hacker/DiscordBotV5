using System.Collections.Generic;

namespace HoLLy.DiscordBot.Parsers
{
    internal interface ITokenizer<T> where T : IToken
    {
        IEnumerable<T> Tokenize(string input);
    }
}
