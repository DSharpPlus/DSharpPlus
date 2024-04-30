namespace DSharpPlus.Net.Abstractions;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Newtonsoft.Json;

public sealed class PollCreatePayload
{
    /// <summary>
    /// Gets the question for this poll. Only text is supported.
    /// </summary>
    [JsonProperty("question")]
    public DiscordPollMedia Question { get; internal set; }

    /// <summary>
    /// Gets the answers available in the poll.
    /// </summary>
    [JsonProperty("answers")]
    public IReadOnlyList<DiscordPollAnswer> Answers { get; internal set; }

    /// <summary>
    /// Gets the expiry date for this poll.
    /// </summary>
    [JsonProperty("duration")]
    public int Duration { get; internal set; }

    /// <summary>
    /// Whether the poll allows for multiple answers.
    /// </summary>
    [JsonProperty("allow_multiselect")]
    public bool AllowMultisect { get; internal set; }

    /// <summary>
    /// Gets the layout type for this poll. Defaults to <see cref="DiscordPollLayoutType.Default"/>.
    /// </summary>
    [JsonProperty("layout_type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPollLayoutType? Layout { get; internal set; }

    internal PollCreatePayload() { }

    internal PollCreatePayload(DiscordPoll poll)
    {
        Question = poll.Question;
        Answers = poll.Answers;
        AllowMultisect = poll.AllowMultisect;
        Layout = poll.Layout;
    }

    internal PollCreatePayload(DiscordPollBuilder builder)
    {
        Question = new DiscordPollMedia { Text = builder.Question };
        Answers = builder.Options.Select(x => new DiscordPollAnswer { AnswerData = x }).ToList();
        AllowMultisect = builder.IsMultipleChoice;
        Duration = builder.Duration;
    }
}
