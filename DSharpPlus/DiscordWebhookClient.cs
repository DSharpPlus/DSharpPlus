using DSharpPlus.Entities;
using DSharpPlus.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using System.Text.RegularExpressions;

namespace DSharpPlus
{
    public class DiscordWebhookClient
    {
        const string WebhookRegex = "(https?:\\/\\/)?discordapp.com\\/api\\/webhooks\\/(.+)\\/(.+)";
        public IReadOnlyList<DiscordWebhook> Webhooks { get; }
        internal List<DiscordWebhook> _hooks;
        internal DiscordApiClient _apiclient;
        internal string _username;
        internal string _avatar_url;

        public DiscordWebhookClient(string username, string avatar_url)
        {
            this._apiclient = new DiscordApiClient();
            this._hooks = new List<DiscordWebhook>();
            Webhooks = _hooks;
            this._username = username;
            this._avatar_url = avatar_url;
        }

        public async Task AddWebhookAsync(ulong id, string token)
        {
            if (_hooks.Any(x => x.Id == id))
                throw new ArgumentException("This webhook is already part of this client!");

            var wh = await _apiclient.GetWebhookWithTokenAsync(id, token);
            _hooks.Add(wh);
        }

        public async Task AddWebhookAsync(string url)
        {
            var m = Regex.Match(url, WebhookRegex);
            var id = m.Groups[2];
            var token = m.Groups[3];
            ulong idulong = ulong.Parse(id.Value);

            await AddWebhookAsync(idulong, token.Value);
        }

        public void AddWebhook(DiscordWebhook webhook)
        {
            if (_hooks.Any(x => x.Id == webhook.Id))
                throw new ArgumentException("This webhook is already part of this client!");

            var nwh = new DiscordWebhook()
            {
                ApiClient = _apiclient,
                AvatarHash = webhook.AvatarHash,
                ChannelId = webhook.ChannelId,
                GuildId = webhook.GuildId,
                Id = webhook.Id,
                Name = webhook.Name,
                Token = webhook.Token,
                User = webhook.User,
                Discord = null
            };
            _hooks.Add(nwh);
        }

        public async Task AddWebhookAsync(ulong id, BaseDiscordClient client)
        {
            if (_hooks.Any(x => x.Id == id))
                throw new ArgumentException("This webhook is already part of this client!");

            var wh = await client.ApiClient.GetWebhookAsync(id);
            var nwh = new DiscordWebhook()
            {
                ApiClient = _apiclient,
                AvatarHash = wh.AvatarHash,
                ChannelId = wh.ChannelId,
                GuildId = wh.GuildId,
                Id = wh.Id,
                Name = wh.Name,
                Token = wh.Token,
                User = wh.User,
                Discord = null
            };
            _hooks.Add(nwh);
        }

        public void RemoveWebhook(ulong id)
        {
            if (!_hooks.Any(x => x.Id == id))
                throw new ArgumentException("This webhook is not part of this client!");

            _hooks.RemoveAll(x => x.Id == id);
        }

        public async Task BroadcastMessageAsync(string content = null, List<DiscordEmbed> embeds = null, bool tts = false, string username_override = null, string avatar_override = null)
        {
            var deadhooks = new List<DiscordWebhook>();
            foreach(var hook in _hooks)
            {
                try
                {
                    await hook.ExecuteAsync(content, username_override ?? _username, avatar_override ?? _avatar_url, tts, embeds);
                }
                catch (NotFoundException)
                {
                    deadhooks.Add(hook);
                }
            }
            // Removing dead webhooks from collection
            _hooks.RemoveAll(x => deadhooks.Any(xx => x.Id == xx.Id));
        }
    }
}
