using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp.Objects;

namespace SharpCord.Toolbox
{
    public static class Tools
    {
        /// <summary>
        /// Takes a DiscordServer and provides a list of DiscordMembers with admin permissions.
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static List<DiscordMember> AdminList(DiscordServer server)
        {
            List<DiscordMember> admins = new List<DiscordMember>();
            foreach (var member in server.membersAsList)
            {
                if (member.HasPermission(DiscordSpecialPermissions.Administrator))
                {
                    admins.Add(member);
                }
            }
            if (admins != null)
                return admins;

            return null;
        }
    }
}
