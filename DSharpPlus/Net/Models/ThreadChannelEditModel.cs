using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models
{
    public class ThreadChannelEditModel : ChannelEditModel
    {
        /// <summary>
        /// Sets if the thread is archived
        /// </summary>
        public bool? IsArchived { internal get; set; }

        /// <summary>
        /// Sets AutoArchiveDuration of the thread
        /// </summary>
        public AutoArchiveDuration? AutoArchiveDuration { internal get; set; }

        /// <summary>
        /// Sets if anyone can unarchive a thread
        /// </summary>
        public bool? Locked { internal get; set; }

        /// <summary>
        /// Sets the applied tags for the thread
        /// </summary>
        public IEnumerable<ulong> AppliedTags { internal get; set; }

        /// <summary>
        /// Sets the flags for the channel (Either PINNED or REQUIRE_TAG)
        /// </summary>
        public ChannelFlags? Flags { internal get; set; }

        internal ThreadChannelEditModel() { }
    }
}
