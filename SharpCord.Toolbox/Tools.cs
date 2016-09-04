using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCord.Objects;

namespace SharpCord.Toolbox
{
    public static class Tools
    {
        /// <summary>
        /// Takes a DiscordServer and provides a list of DiscordMembers with admin permissions.
        /// </summary>
        /// <param name="server">The discord server to sample from</param>
        /// <returns></returns>
        public static List<DiscordMember> AdminList(DiscordServer server)
        {
            return server.membersAsList.Where(t => t.HasPermission(DiscordSpecialPermissions.Administrator)).ToList();
        }
        /// <summary>
        /// Returns the owner of each server in a list of DiscordMembers.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static List<DiscordMember> Owners(DiscordClient client)
        {
            return client.GetServersList().Select(t => t.Owner).ToList();
        }
    }
}
