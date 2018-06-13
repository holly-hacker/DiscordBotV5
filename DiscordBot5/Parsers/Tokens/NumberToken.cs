namespace HoLLy.DiscordBot.Parsers.Tokens
{
    internal class NumberToken : BasicToken
    {
        public int Value { get; }

        public NumberToken(int num) : base(num.ToString())
        {
            Value = num;
        }
    }
}
