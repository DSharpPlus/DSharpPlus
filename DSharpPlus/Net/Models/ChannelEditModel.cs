using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Models
{
    public class ChannelEditModel : BaseEditModel
    {
        /// <summary>
        /// New channel name
        /// </summary>
        public string Name { internal get; set; }
        /// <summary>
        /// New channel position
        /// </summary>
        public int? Position { internal get; set; }
        /// <summary>
        /// New channel topic
        /// </summary>
        public string Topic { internal get; set; }
        /// <summary>
        /// Whether the channel should be marked as NSFW
        /// </summary>
        public bool? Nsfw { internal get; set; }
        /// <summary>
        /// New channel parent (should probably be a category)
        /// </summary>
        public Optional<DiscordChannel> Parent { internal get; set; }
        /// <summary>
        /// New voice channel bitrate
        /// </summary>
        public int? Bitrate { internal get; set; }
        /// <summary>
        /// New voice channel user limit
        /// </summary>
        public int? Userlimit { internal get; set; }
        
        internal ChannelEditModel()
        {

        }
    }
}
