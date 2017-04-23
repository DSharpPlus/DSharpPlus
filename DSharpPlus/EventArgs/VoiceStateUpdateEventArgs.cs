namespace DSharpPlus
{
    public class VoiceStateUpdateEventArgs : DiscordEventArgs
    {
        public ulong UserID { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordUser User => this.Client._rest_client.InternalGetUser(UserID.ToString()).GetAwaiter().GetResult();
        internal string SessionID { get; set; }

        public VoiceStateUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
