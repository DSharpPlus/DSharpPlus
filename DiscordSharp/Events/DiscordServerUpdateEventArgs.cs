using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp.Objects;
namespace DiscordSharp.Events
{
    public class DiscordServerUpdateEventArgs : EventArgs
    {
        public DiscordServer NewServer { get; set; }
        public DiscordServer OldServer { get; set; }
    }
}
