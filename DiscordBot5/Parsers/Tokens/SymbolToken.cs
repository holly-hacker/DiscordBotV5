namespace HoLLy.DiscordBot.Parsers.Tokens
{
    internal class SymbolToken : BasicToken
    {
        public char Value { get; }

        public SymbolToken(char c) : base(c.ToString())
        {
            Value = c;
        }
    }
}
