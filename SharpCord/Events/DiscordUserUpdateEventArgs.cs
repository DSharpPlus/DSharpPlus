using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Objects;
namespace DSharpPlus.Events
{
    public class DiscordUserUpdateEventArgs : EventArgs
    {
        public DiscordMember OriginalMember { get; internal set; }
        public DiscordMember NewMember { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}
