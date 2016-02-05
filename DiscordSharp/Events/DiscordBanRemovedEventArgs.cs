using System;
using DiscordSharp.Objects;

namespace DiscordSharp
{
    public class DiscordBanRemovedEventArgs : EventArgs
    {
        public DiscordServer Guild { get; internal set; }
        public DiscordMember MemberStub { get; internal set; }
    }
}