using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DiscordSharp.Objects
{
    public class DiscordServer
    {
        public string id { get; internal set; }
        public string name { get; internal set; }

        public string region { get; internal set; }

        internal string icon { get; set; }
        public string IconURL
        {
            get
            {
                if (icon != null)
                    return Endpoints.ContentDeliveryNode + Endpoints.Icons + $"/{id}/{icon}.jpg";
                return null;
            }
        }

#pragma warning disable 0612
        private DiscordMember _owner;
        public DiscordMember owner
        {
            get { return _owner; }
            internal set
            {
                _owner = value;
            }
        }
#pragma warning restore 0612

        public List<DiscordChannel> channels { get; internal set; }
        public List<DiscordMember> members { get; internal set; }
        public List<DiscordRole> roles { get; internal set; }

        internal DiscordClient parentclient { get; set; }

        internal DiscordServer()
        {
            channels = new List<DiscordChannel>();
            members = new List<DiscordMember>();
        }

        public void ChangeIcon(Bitmap image)
        {
            Bitmap resized = new Bitmap((Image)image, 200, 200);

            string base64 = Convert.ToBase64String(Utils.ImageToByteArray(resized));
            string type = "image/jpeg;base64";
            string req = $"data:{type},{base64}";
            string guildjson = JsonConvert.SerializeObject(new { icon = req, name = this.name });
            string url = Endpoints.BaseAPI + Endpoints.Guilds + "/" + this.id;
            var result = JObject.Parse(WebWrapper.Patch(url, DiscordClient.token, guildjson));
        }

        public void ChangeName(string NewGuildName)
        {
            string editGuildUrl = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.id}";
            var newNameJson = JsonConvert.SerializeObject(new { name = NewGuildName });
            var result = JObject.Parse(WebWrapper.Patch(editGuildUrl, DiscordClient.token, newNameJson));
        }

        public void AssignRoleToMember(DiscordRole role, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.id}" + Endpoints.Members + $"/{member.ID}";
            string message = JsonConvert.SerializeObject(new { roles = new string[] { role.id } });
            Console.WriteLine(WebWrapper.Patch(url, DiscordClient.token, message));
        }
        public void AssignRoleToMember(List<DiscordRole> roles, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.id}" + Endpoints.Members + $"/{member.ID}";
            List<string> rolesAsIds = new List<string>();
            roles.ForEach(x => rolesAsIds.Add(x.id));
            string message = JsonConvert.SerializeObject(new { roles = rolesAsIds.ToArray() });
            Console.WriteLine(WebWrapper.Patch(url, DiscordClient.token, message));
        }

        public DiscordRole CreateRole()
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.id}" + Endpoints.Roles;
            var result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, ""));

            if (result != null)
            {
                DiscordRole d = new DiscordRole
                {
                    color = new Color(result["color"].ToObject<int>().ToString("x")),
                    hoist = result["hoist"].ToObject<bool>(),
                    id = result["id"].ToString(),
                    managed = result["managed"].ToObject<bool>(),
                    name = result["name"].ToString(),
                    permissions = new DiscordPermission(result["permissions"].ToObject<uint>()),
                    position = result["position"].ToObject<int>()
                };

                this.roles.Add(d);
                return d;
            }
            return null;
        }

        public DiscordRole EditRole(DiscordRole role)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.id}" + Endpoints.Roles + $"/{role.id}";
            string request = JsonConvert.SerializeObject(
                new
                {
                    color = decimal.Parse(role.color.ToDecimal().ToString()),
                    hoist = role.hoist,
                    name = role.name,
                    permissions = role.permissions.GetRawPermissions()
                }
            );

            var result = JObject.Parse(WebWrapper.Patch(url, DiscordClient.token, request));
            if (result != null)
            {
                DiscordRole d = new DiscordRole
                {
                    color = new Color(result["color"].ToObject<int>().ToString("x")),
                    hoist = result["hoist"].ToObject<bool>(),
                    id = result["id"].ToString(),
                    managed = result["managed"].ToObject<bool>(),
                    name = result["name"].ToString(),
                    permissions = new DiscordPermission(result["permissions"].ToObject<uint>()),
                    position = result["position"].ToObject<int>()
                };

                this.roles.Remove(d);
                this.roles.Add(d);
                return d;
            }

            return null;
        }

        public void DeleteRole(DiscordRole role)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.id}" + Endpoints.Roles + $"/{role.id}";
            WebWrapper.Delete(url, DiscordClient.token);
        }

        public void DeleteChannel(DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}";
            WebWrapper.Delete(url, DiscordClient.token);
        }

        public DiscordChannel CreateChannel(string ChannelName, bool voice)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{this.id}" + Endpoints.Channels;
            var reqJson = JsonConvert.SerializeObject(new { name = ChannelName, type = voice ? "voice" : "text" });
            var result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, reqJson));
            if (result != null)
            {
                DiscordChannel dc = new DiscordChannel { Name = result["name"].ToString(), ID = result["id"].ToString(), Type = result["type"].ToObject<ChannelType>(), Private = result["is_private"].ToObject<bool>(), Topic = result["topic"].ToString() };
                this.channels.Add(dc);
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
            List<DiscordMember> returnVal = new List<DiscordMember>();
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{id}" + Endpoints.Bans;
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
                parentclient.GetTextClientLogger.Log($"An error ocurred while retrieving bans for server \"{name}\"\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}",
                    MessageLevel.Error);
            }
            return returnVal;
        }

        public void RemoveBan(string userID)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{id}" + Endpoints.Bans + $"/{userID}";
            try
            {
                WebWrapper.Delete(url, DiscordClient.token);
            }
            catch (Exception ex)
            {
                parentclient.GetTextClientLogger.Log($"Error during RemoveBan\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Error);
            }
        }
        public void RemoveBan(DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{id}" + Endpoints.Bans + $"/{member.ID}";
            try
            {
                WebWrapper.Delete(url, DiscordClient.token);
            }
            catch (Exception ex)
            {
                parentclient.GetTextClientLogger.Log($"Error during RemoveBan\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Error);
            }
        }
    }
}
