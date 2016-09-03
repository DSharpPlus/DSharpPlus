using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordConnectEventArgs : EventArgs
    {
        public DiscordMember User { get; internal set; }
    }
}