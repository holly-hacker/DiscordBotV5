namespace HoLLy.DiscordBot.Parsers.Tokens
{
    internal class NumberToken : BasicToken
    {
        public ulong Value { get; }

        public NumberToken(ulong num) : base(num.ToString())
        {
            Value = num;
        }
    }
}
