namespace DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class DiscordPoll
{
    /// <summary>
    /// Gets the question for this poll. Only text is supported.
    /// </summary>
    public DiscordPollMedia Question { get; internal set; }
    
    /// <summary>
    /// Gets the answers available in the poll.
    /// </summary>
    public IReadOnlyList<DiscordPollAnswer> Answers { get; internal set; }
    
    /// <summary>
    /// Gets the expiry date for this poll.
    /// </summary>
    public DateTimeOffset Expiry { get; internal set; }
    
    /// <summary>
    /// Whether the poll allows for multiple answers.
    /// </summary>
    public bool AllowMultisect { get; internal set; }
    
    /// <summary>
    /// Gets the layout type for this poll. Defaults to <see cref="DiscordPollLayoutType.Default"/>.
    /// </summary>
    [JsonProperty("layout_type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPollLayoutType Layout { get; internal set; }
    
    internal DiscordPoll() { }
}
