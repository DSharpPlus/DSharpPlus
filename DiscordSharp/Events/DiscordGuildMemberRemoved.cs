using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp.Objects;
namespace DiscordSharp.Events
{
    public class DiscordGuildMemberRemovedEventArgs : EventArgs
    {
        public DiscordMember MemberRemoved { get; internal set; }
        public DiscordServer Server { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}
