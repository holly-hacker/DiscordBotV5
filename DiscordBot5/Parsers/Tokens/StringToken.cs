namespace HoLLy.DiscordBot.Parsers.Tokens
{
    internal class StringToken : BasicToken
    {
        public readonly char Boundary;
        public string WithBoundaries => Boundary + TextValue + Boundary;

        public StringToken(string s, char b) : base(s)
        {
            Boundary = b;
        }

        public override string ToString() => WithBoundaries;
    }
}
