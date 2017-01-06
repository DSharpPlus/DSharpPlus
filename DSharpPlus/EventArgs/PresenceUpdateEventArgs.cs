using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class PresenceUpdateEventArgs : EventArgs
    {
        public ulong UserID;
        public DiscordUser User => DiscordClient._guilds[GuildID].Members.Find(x => x.User.ID == UserID).User;
        public List<ulong> RoleIDs;
        public string Game;
        public ulong GuildID;
        public string Status;
    }
}
