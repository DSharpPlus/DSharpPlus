using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Net.Models
{
    public class WelcomeScreenEditModel
    {
        public Optional<bool> Enabled { internal get; set; }

        public Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> WelcomeChannels { internal get; set; }

        public Optional<string> Description { internal get; set; }
    }
}
