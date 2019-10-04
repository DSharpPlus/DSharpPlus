﻿using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Models
{
    public class MemberEditModel : BaseEditModel
    {
        /// <summary>
        /// New nickname
        /// </summary>
        public Optional<string> Nickname { internal get; set; }
        /// <summary>
        /// New roles
        /// </summary>
        public Optional<List<DiscordRole>> Roles { internal get; set; }
        /// <summary>
        /// Whether this user should be muted
        /// </summary>
        public Optional<bool> Muted { internal get; set; }
        /// <summary>
        /// Whether this user should be deafened
        /// </summary>
        public Optional<bool> Deafened { internal get; set; }
        /// <summary>
        /// Voice channel to move this user to, set to null to kick
        /// </summary>
        public Optional<DiscordChannel> VoiceChannel { internal get; set; }
        
        internal MemberEditModel()
        {

        }
    }
}
