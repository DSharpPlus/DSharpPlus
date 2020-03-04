using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Handles mentionables
    /// </summary>
    internal class DiscordMentions
    {
        //https://discordapp.com/developers/docs/resources/channel#allowed-mentions-object

        /// <summary>
        /// Collection roles to serialize
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> Roles { get; }

        /// <summary>
        /// Collection of users to serialize
        /// </summary>
        [JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> Users { get; }

        /// <summary>
        /// The values to be parsed
        /// </summary>
        [JsonProperty("parse", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Parse { get; }

        internal DiscordMentions(IEnumerable<IMention> mentions)
        {
            HashSet<ulong> roles = new HashSet<ulong>();
            HashSet<ulong> users = new HashSet<ulong>();
            HashSet<string> parse = new HashSet<string>();

            foreach(var m in mentions)
            {
                switch(m)
                {
                    case UserMention u:
                        users.Add(u.Id);
                        parse.Add("users");
                        break;

                    case RoleMention r:
                        roles.Add(r.Id);
                        parse.Add("roles");
                        break;

                    case EveryoneMention e:
                        parse.Add("everyone");
                        break;
                }
            }

            Roles = roles;
            Users = users;
            Parse = parse;
        }
    }
}
