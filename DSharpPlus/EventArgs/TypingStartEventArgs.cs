namespace DSharpPlus
{
    public class TypingStartEventArgs : DiscordEventArgs
    {
        public ulong ChannelID { get; internal set; }
        public ulong UserID { get; internal set; }
        public DiscordChannel Channel => this.Client.Guilds[this.Client.GetGuildIdFromChannelID(ChannelID)].Channels.Find(x => x.Id == ChannelID);
        public DiscordMember User => this.Client.Guilds[this.Client.GetGuildIdFromChannelID(ChannelID)].Members.Find(x => x.Id == UserID);

        public TypingStartEventArgs(DiscordClient client) : base(client) { }
    }
}
