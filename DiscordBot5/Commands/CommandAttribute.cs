using System;

namespace HoLLy.DiscordBot.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        internal string Command { get; }
        internal string Description { get; }
        public int? MinPermission { get; }

        public CommandAttribute(string command, string description = null)
        {
            Command = command;
            Description = description;
            MinPermission = null;
        }

        public CommandAttribute(int minPermission, string command, string description = null)
        {
            MinPermission = minPermission;
            Command = command;
            Description = description;
        }
    }
}
