using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ID = System.String;

namespace DSharpPlus.Objects
{
    public class DiscordServer
    {
        /// <summary>
        /// Bot's join date
        /// </summary>
        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; internal set; }

        /// <summary>
        /// Server ID
        /// </summary>
        [JsonProperty("id")]
        public ID ID { get; internal set; }

        /// <summary>
        /// Server Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Server Region
        /// </summary>
        [JsonProperty("region")]
        public string Region { get; internal set; }

        /// <summary>
        /// If true, then the server is currently unavailable and normal events cannot occur.
        /// </summary>
        [JsonProperty("unavailable")]
        public bool Unavailable { get; internal set; } = false;

        /// <summary>
        /// Server Icon
        /// </summary>
        [JsonProperty("icon")]
        public string icon { get; set; }

        public string IconURL
        {
            get
            {
                return $"{Endpoints.BaseAPI}{Endpoints.Guilds}{ID}{Endpoints.Icons}/{icon}.jpg";
            }
        }

#pragma warning disable 0612
        private DiscordMember _owner;

        /// <summary>
        /// Server owner
        /// </summary>
        [JsonProperty("owner")]
        public DiscordMember Owner
        {
            get { return _owner; }
            internal set
            {
                _owner = value;
            }
        }
#pragma warning restore 0612

        /// <summary>
        /// Server Channels
        /// </summary>
        [JsonProperty("channels")]
        public List<DiscordChannel> Channels { get; internal set; }

        /// <summary>
        /// Server Members in a list
        /// </summary>
        [JsonProperty("members")]
        public List<DiscordMember> membersAsList
        {
            get
            {
                return Members.Values.ToList();
            }
        }

        /// <summary>
        /// Server Members
        /// </summary>
        public Dictionary<ID, DiscordMember> Members { get; internal set; }

        //public List<DiscordMember> Members { get; internal set; }

        /// <summary>
        /// Server's roles
        /// </summary>
        [JsonProperty("roles")]
        public List<DiscordRole> Roles { get; internal set; }

        internal DiscordClient parentclient { get; set; }

        internal DiscordServer()
        {
            Channels = new List<DiscordChannel>();
            Members = new Dictionary<ID, DiscordMember>();
        }

        
        internal void AddMember(DiscordMember member)
        {
            if (member == null)
                return;
            if(Members.ContainsKey(member.ID)) //then replace
            {
                Members.Remove(member.ID);
            }
            Members.Add(member.ID, member);
        }

        internal int ClearOfflineMembers()
        {
            int count = 0;
            foreach(var member in Members)
            {
                if (member.Value.Status == Status.Offline)
                    return count;
            }
            return count;
        }
        internal bool RemoveMember(ID key)
        {
            if(Members.ContainsKey(key))
            {
                Members.Remove(key);
            }
            return false;
        }


        public DiscordMember GetMemberByKey(ID key)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            try
            {
                return Members.First(x => x.Key == key).Value;
            }
            catch
            {
                return null; //because instead of just returning null by default, it has to do this shit.
            }
        }

        /// <summary>
        /// Get a user by its username
        /// </summary>
        /// <param name="username">Users's username.</param>
        /// <param name="caseSensitive">Is the username case sensitive?</param>
        public DiscordMember GetMemberByUsername(string username, bool caseSensitive = false)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            if (!caseSensitive)
                return Members.First(x => x.Value.Username.ToLower() == username.ToLower()).Value;
            else
                return Members.First(x => x.Value.Username == username).Value;
        }

        /// <summary>
        /// Changes a servers icon
        /// </summary>
        /// <param name="image">New icon.</param>
        public void ChangeIcon(Bitmap image)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            Bitmap resized = new Bitmap((Image)image, 200, 200);

            string base64 = Convert.ToBase64String(Utils.ImageToByteArray(resized));
            string type = "image/jpeg;base64";
            string req = $"data:{type},{base64}";
            string guildjson = JsonConvert.SerializeObject(new { icon = req, name = this.Name });
            string url = Endpoints.BaseAPI + Endpoints.Guilds + "/" + this.ID;
            var result = JObject.Parse(WebWrapper.Patch(url, DiscordClient.token, guildjson));
        }

        /// <summary>
        /// Changes this servers name
        /// </summary>
        /// <param name="NewGuildName">New name for this server.</param>
        public void ChangeName(string NewGuildName)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string editGuildUrl = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.ID}";
            var newNameJson = JsonConvert.SerializeObject(new { name = NewGuildName });
            var result = JObject.Parse(WebWrapper.Patch(editGuildUrl, DiscordClient.token, newNameJson));
        }

        /// <summary>
        /// Assigns a role to a member of this current server
        /// </summary>
        /// <param name="role">The role you wish to grant.</param>
        /// <param name="member">The member you wish to grant a role</param>
        public void AssignRoleToMember(DiscordRole role, DiscordMember member)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.ID}" + Endpoints.Members + $"/{member.ID}";
            string message = JsonConvert.SerializeObject(new { roles = new string[] { role.ID } });
            Console.WriteLine(WebWrapper.Patch(url, DiscordClient.token, message));
        }

        /// <summary>
        /// Assigns multiple roles to a member of this current server
        /// </summary>
        /// <param name="roles">The roles you wish to grant.</param>
        /// <param name="member">The member you wish to grant a role</param>
        public void AssignRoleToMember(List<DiscordRole> roles, DiscordMember member)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.ID}" + Endpoints.Members + $"/{member.ID}";
            List<string> rolesAsIds = new List<string>();
            roles.ForEach(x => rolesAsIds.Add(x.ID));
            string message = JsonConvert.SerializeObject(new { roles = rolesAsIds.ToArray() });
            Console.WriteLine(WebWrapper.Patch(url, DiscordClient.token, message));
        }
        /// <summary>
        /// Assigns multiple roles to a member of this current server
        /// </summary>
        /// <param name="roles">The roles you wish to grant.</param>
        /// <param name="member">The member you wish to grant a role</param>
        public void AssignRolesToMember(DiscordMember member, params DiscordRole[] roles)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.ID}" + Endpoints.Members + $"/{member.ID}";
            List<string> rolesAsIds = new List<string>();
            roles.ToList().ForEach(x => rolesAsIds.Add(x.ID));
            string message = JsonConvert.SerializeObject(new { roles = rolesAsIds.ToArray() });
            Console.WriteLine(WebWrapper.Patch(url, DiscordClient.token, message));
        }

        /// <summary>
        /// Creates an empty role
        /// </summary>
        public DiscordRole CreateRole()
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.ID}" + Endpoints.Roles;
            var result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, ""));

            if (result != null)
            {
                DiscordRole d = new DiscordRole
                {
                    Color = new Color(result["color"].ToObject<int>().ToString("x")),
                    Hoist = result["hoist"].ToObject<bool>(),
                    ID = result["id"].ToString(),
                    Managed = result["managed"].ToObject<bool>(),
                    Name = result["name"].ToString(),
                    Permissions = new DiscordPermission(result["permissions"].ToObject<uint>()),
                    Position = result["position"].ToObject<int>()
                };

                this.Roles.Add(d);
                return d;
            }
            return null;
        }

        /// <summary>
        /// Edits a role.
        /// </summary>
        /// <param name="role">Role you wish to edit.</param>
        public DiscordRole EditRole(DiscordRole role)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.ID}" + Endpoints.Roles + $"/{role.ID}";
            string request = JsonConvert.SerializeObject(
                new
                {
                    color = decimal.Parse(role.Color.ToDecimal().ToString()),
                    hoist = role.Hoist,
                    name = role.Name,
                    permissions = role.Permissions.GetRawPermissions()
                }
            );

            var result = JObject.Parse(WebWrapper.Patch(url, DiscordClient.token, request));
            if (result != null)
            {
                DiscordRole d = new DiscordRole
                {
                    Color = new Color(result["color"].ToObject<int>().ToString("x")),
                    Hoist = result["hoist"].ToObject<bool>(),
                    ID = result["id"].ToString(),
                    Managed = result["managed"].ToObject<bool>(),
                    Name = result["name"].ToString(),
                    Permissions = new DiscordPermission(result["permissions"].ToObject<uint>()),
                    Position = result["position"].ToObject<int>()
                };

                this.Roles.Remove(d);
                this.Roles.Add(d);
                return d;
            }

            return null;
        }

        /// <summary>
        /// Deletes a role.
        /// </summary>
        /// <param name="role">Role you wish to delete.</param>
        public void DeleteRole(DiscordRole role)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.ID}" + Endpoints.Roles + $"/{role.ID}";
            WebWrapper.Delete(url, DiscordClient.token);
        }

        /// <summary>
        /// Deletes a channel.
        /// </summary>
        /// <param name="channel">Channel you wish to edit.</param>
        public void DeleteChannel(DiscordChannel channel)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}";
            WebWrapper.Delete(url, DiscordClient.token);
        }

        /// <summary>
        /// Creates a channel.
        /// </summary>
        /// <param name="ChannelName">New channel's name.</param>
        /// <param name="voice">Wether this channel is a voice channel</param>
        public DiscordChannel CreateChannel(string ChannelName, bool voice)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.ID}" + Endpoints.Channels;
            var reqJson = JsonConvert.SerializeObject(new { name = ChannelName, type = voice ? "voice" : "text" });
            var result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, reqJson));
            if (result != null)
            {
                DiscordChannel dc = new DiscordChannel {
                    Name = result["name"].ToString(),
                    ID = result["id"].ToString(),
                    Type = result["type"].ToObject<ChannelType>(),
                    Private = result["is_private"].ToObject<bool>()
                };
                if (!result["topic"].IsNullOrEmpty())
                {
                    dc.Topic = result["topic"].ToString();
                }
                if (dc.Type == ChannelType.Voice && !result["bitrate"].IsNullOrEmpty())
                {
                    dc.Bitrate = result["bitrate"].ToObject<int>();
                }
                this.Channels.Add(dc);
                return dc;
            }
            return null;
        }

        /// <summary>
        /// Retrieves a DiscordMember List of members banned in this server.
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public List<DiscordMember> GetBans()
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            List<DiscordMember> returnVal = new List<DiscordMember>();
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{ID}" + Endpoints.Bans;
            try
            {
                JArray response = JArray.Parse(WebWrapper.Get(url, DiscordClient.token));
                if (response != null && response.Count > 0)
                {
                    parentclient.GetTextClientLogger.Log($"Ban count: {response.Count}");

                    foreach (var memberStub in response)
                    {
                        DiscordMember temp = JsonConvert.DeserializeObject<DiscordMember>(memberStub["user"].ToString());
                        if (temp != null)
                            returnVal.Add(temp);
                        else
                            parentclient.GetTextClientLogger.Log($"memberStub[\"user\"] was null?! Username: {memberStub["user"]["username"].ToString()} ID: {memberStub["user"]["username"].ToString()}", MessageLevel.Error);
                    }
                }
                else
                    return returnVal;
            }
            catch (Exception ex)
            {
                parentclient.GetTextClientLogger.Log($"An error ocurred while retrieving bans for server \"{Name}\"\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}",
                    MessageLevel.Error);
            }
            return returnVal;
        }

        /// <summary>
        /// Removes a ban.
        /// </summary>
        /// <param name="userID">User ID you wish to unban</param>
        public void RemoveBan(string userID)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{ID}" + Endpoints.Bans + $"/{userID}";
            try
            {
                WebWrapper.Delete(url, DiscordClient.token);
            }
            catch (Exception ex)
            {
                parentclient.GetTextClientLogger.Log($"Error during RemoveBan\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Error);
            }
        }
        /// <summary>
        /// Removes a ban.
        /// </summary>
        /// <param name="member">member you wish to unban</param>
        public void RemoveBan(DiscordMember member)
        {
            if (Unavailable)
                throw new Exception("Server is currently unavailable!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{ID}" + Endpoints.Bans + $"/{member.ID}";
            try
            {
                WebWrapper.Delete(url, DiscordClient.token);
            }
            catch (Exception ex)
            {
                parentclient.GetTextClientLogger.Log($"Error during RemoveBan\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Gets a list of invites.
        /// </summary>
        public List<DiscordInvite> GetInvites()
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{ID    }" + Endpoints.Invites;
            var result = JArray.Parse(WebWrapper.Get(url, DiscordClient.token));
            List<DiscordInvite> invitelist = new List<DiscordInvite>();
            foreach (var child in result)
            {
                invitelist.Add(JsonConvert.DeserializeObject<DiscordInvite>(child.ToString()));
            }
            return invitelist;
        }
        /// <summary>
        /// Copies a server's information
        /// </summary>
        /// <returns></returns>
        public DiscordServer ShallowCopy()
        {
            return (DiscordServer)this.MemberwiseClone();
        }
    }
}
