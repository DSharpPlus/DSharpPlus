using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Net.Models
{
    public class MembershipScreeningEditModel : BaseEditModel
    {
        public bool? Enabled { internal get; set; } = null;
        public Optional<string> Description { internal get; set; }
        public DiscordGuildMembershipScreeningField Terms { internal get; set; } = null;
        internal MembershipScreeningEditModel() { }
    }
}
