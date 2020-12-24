using System;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VideoNext.Entities
{
    internal class VideoSender
    {
        public uint SSRC { get; }
        public ulong Id => this.User?.Id ?? 0;
        public DiscordUser User { get; set; } = null;
        public ushort LastSequence { get; set; } = 0;

        public VideoSender(uint ssrc)
        {
            this.SSRC = ssrc;
        }
        
    }
}