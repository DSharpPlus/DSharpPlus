namespace DSharpPlus.EventArgs
{
    public class SocketEventArgs : DiscordEventArgs
    {
        public SocketEventArgs(DiscordClient client)
            : base(client)
        { }
    }
}
