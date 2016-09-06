using System;
using SharpCord.Objects;

namespace SharpCord
{
    public class DiscordBanRemovedEventArgs : EventArgs
    {
        public DiscordServer Server { get; internal set; }
        public DiscordMember MemberStub { get; internal set; }
    }
}