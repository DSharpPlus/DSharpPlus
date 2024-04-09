namespace DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;

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
    public IReadOnlyList<DiscordPollMedia> Options => this._options;
    private readonly List<DiscordPollMedia> _options = new();
    
    /// <summary>
    /// Gets or sets the expiry date for this poll.
    /// </summary>
    public DateTimeOffset Expiry { get; set; } = DateTimeOffset.UtcNow.AddHours(1);

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

        this._options.Add(new DiscordPollMedia { Text = text, Emoji = emoji });
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
    /// <param name="expiry">When the poll is to expire.</param>
    /// <returns>The modified builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="expiry"/> is in the past or more than 7 days in the future.</exception>
    public DiscordPollBuilder WithExpiry(DateTimeOffset expiry)
    {
        if (expiry < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Expiry date must be in the future.");
        }
        
        if (expiry - DateTimeOffset.UtcNow > TimeSpan.FromDays(7))
        {
            throw new InvalidOperationException("Expiry date must be within 7 days.");
        }
        
        this.Expiry = expiry;
        return this;
    }
    
    /// <summary>
    /// Builds the poll.
    /// </summary>
    /// <returns>A <see cref="DiscordPoll"/> to pass to methods.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the poll has less than two options.</exception>
    public DiscordPoll Build()
    {
        if (this._options.Count < 2)
        {
            throw new InvalidOperationException("A poll must have at least two options.");
        }

        return new DiscordPoll
        {
            Expiry = this.Expiry,
            AllowMultisect = this.IsMultipleChoice,
            Question = new DiscordPollMedia { Text = this.Question },
            Answers = this._options.Select(x => new DiscordPollAnswer { AnswerData = x }).ToList(),
        };
    }
}
