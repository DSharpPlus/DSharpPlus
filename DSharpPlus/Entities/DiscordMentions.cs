using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Handles mentionables
    /// </summary>
    internal class DiscordMentions
    {
        //https://discord.com/developers/docs/resources/channel#allowed-mentions-object

        private const string ParseUsers = "users";
        private const string ParseRoles = "roles";
        private const string ParseEveryone = "everyone";

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
                    default: throw new NotSupportedException("Type not supported in mentions.");
                    case UserMention u:
                        if (u.Id.HasValue) {
                            users.Add(u.Id.Value);      //We have a user ID so we will add them to the implicit
                        } else {
                            parse.Add(ParseUsers);      //We have no ID, so let all users through
                        }

                        break;

                    case RoleMention r:
                        if (r.Id.HasValue) {
                            users.Add(r.Id.Value);      //We have a role ID so we will add them to the implicit
                        } else {
                            parse.Add(ParseRoles);      //We have role ID, so let all users through
                        }
                        break;

                    case EveryoneMention e:
                        parse.Add(ParseEveryone);
                        break;
                }
            }

            //Check the validity of each item. If it isn't in the explicit allow list and they have items, then add them.
            if (!parse.Contains(ParseUsers) && users.Count > 0)
                Users = users;

            if (!parse.Contains(ParseRoles) && roles.Count > 0)
                Roles = roles;

            if (parse.Count > 0)
                Parse = parse;
        }
    }
}
