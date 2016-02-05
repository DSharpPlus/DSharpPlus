using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordGuildBanEventArgs : EventArgs
    {
        public DiscordMember MemberBanned { get; internal set; }
        public DiscordServer Server { get; internal set; }
    }
}