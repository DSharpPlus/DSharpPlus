using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class PresenceUpdateEventArgs : EventArgs
    {
        public DiscordUser User;
        public List<ulong> RoleIDs;
        public string Game;
        public ulong GuildID;
        public string Status;
    }
}
