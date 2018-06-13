namespace HoLLy.DiscordBot.Parsers.Tokens
{
    internal class BasicToken : IToken
    {
        public virtual string TextValue { get; }

        public BasicToken(string s)
        {
            TextValue = s;
        }

        public override string ToString() => TextValue;
    }
}
