namespace DSharpPlus.EventArgs
{
    public class UserSpeakingEventArgs : System.EventArgs
    {
        public ulong UserID { get; internal set; }
        public uint ssrc { get; internal set; }
        public bool Speaking { get; internal set; }
    }
}
