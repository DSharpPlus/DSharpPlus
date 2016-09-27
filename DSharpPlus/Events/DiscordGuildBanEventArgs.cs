using System;
using DSharpPlus.Objects;
namespace DSharpPlus
{
    public class DiscordGuildBanEventArgs : EventArgs
    {
        public DiscordMember MemberBanned { get; internal set; }
        public DiscordServer Server { get; internal set; }
    }
}