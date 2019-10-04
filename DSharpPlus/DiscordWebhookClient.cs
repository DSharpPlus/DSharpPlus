﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a webhook-only client. This client can be used to execute Discord webhooks.
    /// </summary>
    public class DiscordWebhookClient
    {
        // this regex has 2 named capture groups: "id" and "token".
        private static Regex WebhookRegex { get; } = new Regex(@"(?:https?:\/\/)?discordapp.com\/api\/(?:v\d\/)?webhooks\/(?<id>\d+)\/(?<token>[A-Za-z0-9_\-]+)", RegexOptions.ECMAScript);

        /// <summary>
        /// Gets the collection of registered webhooks.
        /// </summary>
        public IReadOnlyList<DiscordWebhook> Webhooks { get; }

        /// <summary>
        /// Gets or sets the username override for registered webhooks. Note that this only takes effect when broadcasting.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or set the avatar override for registered webhooks. Note that this only takes effect when broadcasting.
        /// </summary>
        public string AvatarUrl { get; set; }

        internal List<DiscordWebhook> _hooks;
        internal DiscordApiClient _apiclient;

        /// <summary>
        /// Creates a new webhook client.
        /// </summary>
        public DiscordWebhookClient()
            : this(null)
        { }

        /// <summary>
        /// Creates a new webhook client, with specified HTTP proxy settings.
        /// </summary>
        /// <param name="proxy">Proxy to use for HTTP connections.</param>
        public DiscordWebhookClient(IWebProxy proxy)
            : this(proxy, TimeSpan.FromSeconds(10))
        { }

        /// <summary>
        /// Creates a new webhook client, with specified HTTP proxy and timeout settings.
        /// </summary>
        /// <param name="proxy">Proxy to use for HTTP connections.</param>
        /// <param name="timeout">Timeout to use for HTTP requests. Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable timeouts.</param>
        public DiscordWebhookClient(IWebProxy proxy, TimeSpan timeout)
        {
            this._apiclient = new DiscordApiClient(proxy, timeout);
            this._hooks = new List<DiscordWebhook>();
            this.Webhooks = new ReadOnlyCollection<DiscordWebhook>(this._hooks);
        }

        /// <summary>
        /// Registers a webhook with this client. This retrieves a webhook based on the ID and token supplied.
        /// </summary>
        /// <param name="id">The ID of the webhook to add.</param>
        /// <param name="token">The token of the webhook to add.</param>
        /// <returns>The registered webhook.</returns>
        public async Task<DiscordWebhook> AddWebhookAsync(ulong id, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            token = token.Trim();

            if (_hooks.Any(x => x.Id == id))
                throw new InvalidOperationException("This webhook is registered with this client.");

            var wh = await _apiclient.GetWebhookWithTokenAsync(id, token).ConfigureAwait(false);
            _hooks.Add(wh);

            return wh;
        }

        /// <summary>
        /// Registers a webhook with this client. This retrieves a webhook from webhook URL.
        /// </summary>
        /// <param name="url">URL of the webhook to retrieve. This URL must contain both ID and token.</param>
        /// <returns>The registered webhook.</returns>
        public Task<DiscordWebhook> AddWebhookAsync(Uri url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var m = WebhookRegex.Match(url.ToString());
            if (!m.Success)
                throw new ArgumentException("Invalid webhook URL supplied.", nameof(url));

            var idraw = m.Groups["id"];
            var tokenraw = m.Groups["token"];
            if (!ulong.TryParse(idraw.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                throw new ArgumentException("Invalid webhook URL supplied.", nameof(url));

            var token = tokenraw.Value;
            return AddWebhookAsync(id, token);
        }

        /// <summary>
        /// Registers a webhook with this client. This retrieves a webhook using the supplied full discord client.
        /// </summary>
        /// <param name="id">ID of the webhook to register.</param>
        /// <param name="client">Discord client to which the webhook will belong.</param>
        /// <returns>The registered webhook.</returns>
        public async Task<DiscordWebhook> AddWebhookAsync(ulong id, BaseDiscordClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (_hooks.Any(x => x.Id == id))
                throw new ArgumentException("This webhook is already registered with this client.");

            var wh = await client.ApiClient.GetWebhookAsync(id).ConfigureAwait(false);
            // personally I don't think we need to override anything.
            // it would even make sense to keep the hook as-is, in case
            // it's returned without a token for some bizarre reason
            // remember -- discord is not really consistent
            //var nwh = new DiscordWebhook()
            //{
            //    ApiClient = _apiclient,
            //    AvatarHash = wh.AvatarHash,
            //    ChannelId = wh.ChannelId,
            //    GuildId = wh.GuildId,
            //    Id = wh.Id,
            //    Name = wh.Name,
            //    Token = wh.Token,
            //    User = wh.User,
            //    Discord = null
            //};
            _hooks.Add(wh);

            return wh;
        }

        /// <summary>
        /// Registers a webhook with this client. This reuses the supplied webhook object.
        /// </summary>
        /// <param name="webhook">Webhook to register.</param>
        /// <returns>The registered webhook.</returns>
        public DiscordWebhook AddWebhook(DiscordWebhook webhook)
        {
            if (webhook == null)
                throw new ArgumentNullException(nameof(webhook));

            if (_hooks.Any(x => x.Id == webhook.Id))
                throw new ArgumentException("This webhook is already registered with this client.");

            // see line 110-113 for explanation
            //var nwh = new DiscordWebhook()
            //{
            //    ApiClient = _apiclient,
            //    AvatarHash = webhook.AvatarHash,
            //    ChannelId = webhook.ChannelId,
            //    GuildId = webhook.GuildId,
            //    Id = webhook.Id,
            //    Name = webhook.Name,
            //    Token = webhook.Token,
            //    User = webhook.User,
            //    Discord = null
            //};
            _hooks.Add(webhook);

            return webhook;
        }

        /// <summary>
        /// Unregisters a webhook with this client.
        /// </summary>
        /// <param name="id">ID of the webhook to unregister.</param>
        /// <returns>The unregistered webhook.</returns>
        public DiscordWebhook RemoveWebhook(ulong id)
        {
            if (!_hooks.Any(x => x.Id == id))
                throw new ArgumentException("This webhook is not registered with this client.");

            var wh = this.GetRegisteredWebhook(id);
            this._hooks.Remove(wh);
            return wh;
        }

        /// <summary>
        /// Gets a registered webhook with specified ID.
        /// </summary>
        /// <param name="id">ID of the registered webhook to retrieve.</param>
        /// <returns>The requested webhook.</returns>
        public DiscordWebhook GetRegisteredWebhook(ulong id)
            => this._hooks.FirstOrDefault(xw => xw.Id == id);

        /// <summary>
        /// Broadcasts a message to all registered webhooks.
        /// </summary>
        /// <param name="content">Contents of the message to broadcast.</param>
        /// <param name="embeds">Embeds to send with the messages.</param>
        /// <param name="tts">Whether the messages should be read aloud using TTS engine.</param>
        /// <param name="username_override">Username to use for this broadcast.</param>
        /// <param name="avatar_override">Avatar URL to use for this broadcast.</param>
        /// <param name="file_name">Name of the file to broadcast.</param>
        /// <param name="file_data">Content of the file to broadcast.</param>
        /// <returns></returns>
        public Task BroadcastMessageAsync(string content = null, List<DiscordEmbed> embeds = null, bool tts = false, string username_override = null, string avatar_override = null, string file_name = null, Stream file_data = null)
        {
            return BroadcastMessageAsync(content, embeds, tts, username_override, avatar_override, new Dictionary<string, Stream> { { file_name, file_data } });
        }

        /// <summary>
        /// Broadcasts a message to all registered webhooks.
        /// </summary>
        /// <param name="content">Contents of the message to broadcast.</param>
        /// <param name="embeds">Embeds to send with the messages.</param>
        /// <param name="tts">Whether the messages should be read aloud using TTS engine.</param>
        /// <param name="username_override">Username to use for this broadcast.</param>
        /// <param name="avatar_override">Avatar URL to use for this broadcast.</param>
        /// <param name="files">Files to broadcast.</param>
        /// <returns></returns>
        public async Task BroadcastMessageAsync(string content = null, List<DiscordEmbed> embeds = null, bool tts = false, string username_override = null, string avatar_override = null, Dictionary<string, Stream> files = null)
        {
            var deadhooks = new List<DiscordWebhook>();
            foreach (var hook in _hooks)
            {
                try
                {
                    await hook.ExecuteAsync(content, username_override ?? this.Username, avatar_override ?? this.AvatarUrl, tts, embeds, files).ConfigureAwait(false);
                }
                catch (NotFoundException)
                {
                    deadhooks.Add(hook);
                }
            }
            // Removing dead webhooks from collection
            foreach (var xwh in deadhooks)
                _hooks.Remove(xwh);
        }
    }
}

// 9/11 would improve again
