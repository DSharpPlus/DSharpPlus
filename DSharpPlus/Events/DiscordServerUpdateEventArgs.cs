using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Objects;
namespace DSharpPlus.Events
{
    public class DiscordServerUpdateEventArgs : EventArgs
    {
        public DiscordServer NewServer { get; set; }
        public DiscordServer OldServer { get; set; }
    }
}
