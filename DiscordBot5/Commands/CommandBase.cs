using System;

namespace HoLLy.DiscordBot.Commands
{
    internal abstract class CommandBase
    {
        public readonly string Verb;
        public readonly string Description;
        public readonly int MinPermission;
        public abstract string Usage { get; }

        public CommandBase(string verb, string description, int minPermission = 0)
        {
            Verb = verb.ToLowerInvariant();
            Description = description;
            MinPermission = minPermission;
        }

        public bool Matches(string verb, string arguments) => Verb.Equals(verb, StringComparison.InvariantCultureIgnoreCase) && MatchesArguments(arguments);

        public abstract bool MatchesArguments(string arguments);

        public abstract object Invoke(string arguments);
    }
}
