using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Net.Models
{
    public class MembershipScreeningEditModel : BaseEditModel
    {
        /// <summary>
        /// Sets whether membership screening should be enabled for this guild
        /// </summary>
        public bool? Enabled { internal get; set; } = null;

        /// <summary>
        /// Sets the server description shown in the membership screening form
        /// </summary>
        public Optional<string> Description { internal get; set; }

        /// <summary>
        /// Sets the server rules shown in the membership screening form
        /// </summary>
        public DiscordGuildMembershipScreeningField Terms { internal get; set; } = null;
        internal MembershipScreeningEditModel() { }
    }
}
