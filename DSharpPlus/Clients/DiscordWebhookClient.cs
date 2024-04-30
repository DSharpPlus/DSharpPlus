namespace DSharpPlus;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Metrics;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a webhook-only client. This client can be used to execute Discord webhooks.
/// </summary>
public class DiscordWebhookClient
{
    /// <summary>
    /// Gets the logger for this client.
    /// </summary>
    public ILogger<DiscordWebhookClient> Logger { get; }

    // this regex has 2 named capture groups: "id" and "token".
    private static Regex WebhookRegex { get; } = new Regex(@"(?:https?:\/\/)?discord(?:app)?.com\/api\/(?:v\d\/)?webhooks\/(?<id>\d+)\/(?<token>[A-Za-z0-9_\-]+)", RegexOptions.ECMAScript);

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

    internal LogLevel _minimumLogLevel;
    internal string _logTimestampFormat;

    /// <summary>
    /// Creates a new webhook client.
    /// </summary>
    public DiscordWebhookClient()
        : this(null, null)
    { }

    /// <summary>
    /// Creates a new webhook client, with specified HTTP proxy, timeout, and logging settings.
    /// </summary>
    /// <param name="proxy">Proxy to use for HTTP connections.</param>
    /// <param name="timeout">Timeout to use for HTTP requests. Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable timeouts.</param>
    /// <param name="loggerFactory">The optional logging factory to use for this client.</param>
    /// <param name="minimumLogLevel">The minimum logging level for messages.</param>
    /// <param name="logTimestampFormat">The timestamp format to use for the logger.</param>
    public DiscordWebhookClient(IWebProxy proxy = null, TimeSpan? timeout = null,
        ILoggerFactory loggerFactory = null, LogLevel minimumLogLevel = LogLevel.Information, string logTimestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
    {
        _minimumLogLevel = minimumLogLevel;
        _logTimestampFormat = logTimestampFormat;

        if (loggerFactory == null)
        {
            loggerFactory = new DefaultLoggerFactory();
            loggerFactory.AddProvider(new DefaultLoggerProvider(this));
        }

        Logger = loggerFactory.CreateLogger<DiscordWebhookClient>();

        TimeSpan parsedTimeout = timeout ?? TimeSpan.FromSeconds(10);

        _apiclient = new DiscordApiClient(proxy, parsedTimeout, Logger);
        _hooks = new List<DiscordWebhook>();
        Webhooks = new ReadOnlyCollection<DiscordWebhook>(_hooks);
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
        {
            throw new ArgumentNullException(nameof(token));
        }

        token = token.Trim();

        if (_hooks.Any(x => x.Id == id))
        {
            throw new InvalidOperationException("This webhook is registered with this client.");
        }

        DiscordWebhook wh = await _apiclient.GetWebhookWithTokenAsync(id, token);
        _hooks.Add(wh);

        return wh;
    }

    /// <inheritdoc cref="RestClient.GetRequestMetrics(bool)"/>
    public RequestMetricsCollection GetRequestMetrics(bool sinceLastCall = false)
        => _apiclient.GetRequestMetrics(sinceLastCall);

    /// <summary>
    /// Registers a webhook with this client. This retrieves a webhook from webhook URL.
    /// </summary>
    /// <param name="url">URL of the webhook to retrieve. This URL must contain both ID and token.</param>
    /// <returns>The registered webhook.</returns>
    public Task<DiscordWebhook> AddWebhookAsync(Uri url)
    {
        if (url == null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        Match m = WebhookRegex.Match(url.ToString());
        if (!m.Success)
        {
            throw new ArgumentException("Invalid webhook URL supplied.", nameof(url));
        }

        Group idraw = m.Groups["id"];
        Group tokenraw = m.Groups["token"];
        if (!ulong.TryParse(idraw.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong id))
        {
            throw new ArgumentException("Invalid webhook URL supplied.", nameof(url));
        }

        string token = tokenraw.Value;
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
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (_hooks.Any(x => x.Id == id))
        {
            throw new ArgumentException("This webhook is already registered with this client.");
        }

        DiscordWebhook wh = await client.ApiClient.GetWebhookAsync(id);
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
        {
            throw new ArgumentNullException(nameof(webhook));
        }

        if (_hooks.Any(x => x.Id == webhook.Id))
        {
            throw new ArgumentException("This webhook is already registered with this client.");
        }

        // see line 128-131 for explanation
        // For christ's sake, update the line numbers if they change.
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
        {
            throw new ArgumentException("This webhook is not registered with this client.");
        }

        DiscordWebhook wh = GetRegisteredWebhook(id);
        _hooks.Remove(wh);
        return wh;
    }

    /// <summary>
    /// Gets a registered webhook with specified ID.
    /// </summary>
    /// <param name="id">ID of the registered webhook to retrieve.</param>
    /// <returns>The requested webhook.</returns>
    public DiscordWebhook GetRegisteredWebhook(ulong id)
        => _hooks.FirstOrDefault(xw => xw.Id == id);

    /// <summary>
    /// Broadcasts a message to all registered webhooks.
    /// </summary>
    /// <param name="builder">Webhook builder filled with data to send.</param>
    /// <returns></returns>
    public async Task<Dictionary<DiscordWebhook, DiscordMessage>> BroadcastMessageAsync(DiscordWebhookBuilder builder)
    {
        List<DiscordWebhook> deadhooks = new List<DiscordWebhook>();
        Dictionary<DiscordWebhook, DiscordMessage> messages = new Dictionary<DiscordWebhook, DiscordMessage>();

        foreach (DiscordWebhook hook in _hooks)
        {
            try
            {
                messages.Add(hook, await hook.ExecuteAsync(builder));
            }
            catch (NotFoundException)
            {
                deadhooks.Add(hook);
            }
        }

        // Removing dead webhooks from collection
        foreach (DiscordWebhook xwh in deadhooks)
        {
            _hooks.Remove(xwh);
        }

        return messages;
    }

    ~DiscordWebhookClient()
    {
        _hooks?.Clear();
        _hooks = null!;
        _apiclient._rest.Dispose();
    }
}

// 9/11 would improve again
