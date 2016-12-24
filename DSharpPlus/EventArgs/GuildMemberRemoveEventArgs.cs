namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public DiscordUser User;
    }
}
