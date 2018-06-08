using System;

namespace HoLLy.DiscordBot.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        internal string Command { get; }
        internal string Description { get; }

        public CommandAttribute(string command, string description = "")
        {
            Command = command;
            Description = description;
        }
    }
}
