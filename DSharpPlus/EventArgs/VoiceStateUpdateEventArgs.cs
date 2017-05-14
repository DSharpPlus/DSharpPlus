namespace DSharpPlus
{
    public class VoiceStateUpdateEventArgs : DiscordEventArgs
    {
        public ulong UserID { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordMember Member => this.Client.Guilds[GuildID].Members.Find(x => x.Id == UserID);
        internal string SessionID { get; set; }

        public VoiceStateUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
