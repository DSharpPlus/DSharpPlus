using System;
using System.Collections.Generic;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a builder for <see cref="DiscordPoll"/>s.
/// </summary>
public class DiscordPollBuilder
{
    /// <summary>
    /// Gets or sets the question for this poll.
    /// </summary>
    public string Question { get; set; }

    /// <summary>
    /// Gets or sets whether this poll is multiple choice.
    /// </summary>
    public bool IsMultipleChoice { get; set; }

    /// <summary>
    /// Gets the options for this poll.
    /// </summary>
    public IReadOnlyList<DiscordPollMedia> Options => this.options;
    private readonly List<DiscordPollMedia> options = [];

    /// <summary>
    /// Gets or sets the duration for this poll in hours.
    /// </summary>
    public int Duration { get; set; } = 1;

    /// <summary>
    /// Sets the question for this poll.
    /// </summary>
    /// <param name="question">The question for the poll.</param>
    /// <returns>The modified builder to chain calls with.</returns>
    public DiscordPollBuilder WithQuestion(string question)
    {
        this.Question = question;
        return this;
    }

    /// <summary>
    /// Adds an option to this poll.
    /// </summary>
    /// <param name="text">The text for the option. Null may be passed if <paramref name="emoji"/> is passed instead.</param>
    /// <param name="emoji">An optional emoji for the poll.</param>
    /// <returns>The modified builder to chain calls with.</returns>
    public DiscordPollBuilder AddOption(string text, DiscordComponentEmoji? emoji = null)
    {
        if (emoji is null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(text);
        }

        this.options.Add(new DiscordPollMedia { Text = text, Emoji = emoji });
        return this;
    }

    /// <summary>
    /// Sets whether this poll is multiple choice.
    /// </summary>
    /// <param name="isMultiChoice">Whether the builder is multiple-choice. Defaults to <c>true</c></param>
    /// <returns>The modified builder to chain calls with.</returns>
    public DiscordPollBuilder AsMultipleChoice(bool isMultiChoice = true)
    {
        this.IsMultipleChoice = isMultiChoice;
        return this;
    }

    /// <summary>
    /// Sets the expiry date for this poll.
    /// </summary>
    /// <param name="hours">How many hours the poll should last.</param>
    /// <returns>The modified builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="hours"/> is in the past or more than 7 days in the future.</exception>
    public DiscordPollBuilder WithDuration(int hours)
    {
        if (hours < 1)
        {
            throw new InvalidOperationException("Duration must be at least 1 hour.");
        }

        if (hours > 24 * 7)
        {
            throw new InvalidOperationException("Duration must be less then 7 days/168 hours.");
        }

        this.Duration = hours;
        return this;
    }

    /// <summary>
    /// Builds the poll.
    /// </summary>
    /// <returns>A <see cref="PollCreatePayload"/> to build the create request.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the poll has less than two options.</exception>
    internal PollCreatePayload BuildInternal() => this.options.Count < 2
            ? throw new InvalidOperationException("A poll must have at least two options.")
            : new PollCreatePayload(this);

    public DiscordPollBuilder() { }

    public DiscordPollBuilder(DiscordPoll poll)
    {
        WithQuestion(poll.Question.Text);
        AsMultipleChoice(poll.AllowMultisect);

        foreach (DiscordPollAnswer option in poll.Answers)
        {
            AddOption(option.AnswerData.Text, option.AnswerData.Emoji);
        }
    }
}
