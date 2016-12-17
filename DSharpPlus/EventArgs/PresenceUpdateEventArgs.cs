using System.Collections.Generic;
using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class PresenceUpdateEventArgs : System.EventArgs
    {
        public DiscordUser User;
        public List<ulong> RoleIDs;
        public string Game;
        public ulong GuildID;
        public string Status;
    }
}
