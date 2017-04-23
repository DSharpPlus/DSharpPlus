using System;

namespace DSharpPlus.Commands
{
    public class CommandErrorEventArgs : EventArgs
    {
        public Command Command { get; internal set; }
        public Exception Exception { get; internal set; }

        public DiscordMessage Message { get; internal set; }
        public DiscordChannel Channel => this.Message.Channel;
        public DiscordGuild Guild => this.Channel.Guild;
        public DiscordUser Author => this.Message.Author;
    }
}
