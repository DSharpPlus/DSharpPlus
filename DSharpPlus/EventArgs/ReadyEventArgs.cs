namespace DSharpPlus
{
    public sealed class ReadyEventArgs : DiscordEventArgs
    {
        public ReadyEventArgs(DiscordClient client) : base(client) { }
    }
}
