// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Globalization;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Extensions.Internal.Toolbox;

/// <summary>
/// Provides a helper to deconstruct poll result messages into readable C# code.
/// </summary>
public readonly record struct PollResultMessage
{
    private PollResultMessage
    (
        string questionText,
        int answerVotes,
        int totalVotes,
        int? answerId,
        string? answerText,
        Snowflake? answerEmojiId,
        string? answerEmojiName,
        bool? isAnswerEmojiAnimated
    )
    {
        this.PollQuestionText = questionText;
        this.WinningAnswerVoteCount = answerVotes;
        this.TotalVotes = totalVotes;
        this.WinningAnswerId = answerId;
        this.WinningAnswerText = answerText;
        this.WinningAnswerEmojiId = answerEmojiId;
        this.WinningAnswerEmojiName = answerEmojiName;
        this.IsWinningAnswerEmojiAnimated = isAnswerEmojiAnimated;
    }

    /// <summary>
    /// Attempts to deconstruct a message or partial message into a poll result message.
    /// </summary>
    /// <param name="message">The message object to attempt to deconstruct.</param>
    /// <param name="result">If possible, a C# representation of a poll result message.</param>
    /// <returns>A value indicating whether deconstruction was successful.</returns>
    public static bool TryDeconstructPollResult(IPartialMessage message, out PollResultMessage result)
    {
        if (message.Type != DiscordMessageType.PollResult)
        {
            result = default;
            return false;
        }

        if (message.Embeds is not { HasValue: true, Value: [IEmbed embed] })
        {
            result = default;
            return false;
        }

        if (embed.Type != "poll_result" || !embed.Fields.HasValue)
        {
            result = default;
            return false;
        }

        string questionText = "unknown";
        int answerVotes = 0;
        int totalVotes = 0;
        int? answerId = default;
        string? answerText = default;
        Snowflake? answerEmojiId = default;
        string? answerEmojiName = default;
        bool? isAnswerEmojiAnimated = default;

        // https://discord.com/developers/docs/resources/message#embed-fields-by-embed-type-poll-result-embed-fields
        // i asked the devil whether they understood what the thought process here was, they said no.
        foreach (IEmbedField field in embed.Fields.Value)
        {
            switch (field.Name)
            {
                case "poll_question_text":
                    questionText = field.Value;
                    break;

                case "victor_answer_votes":

                    if (!int.TryParse(field.Value, CultureInfo.InvariantCulture, out answerVotes))
                    {
                        result = default;
                        return false;
                    }

                    break;

                case "total_votes":

                    if (!int.TryParse(field.Value, CultureInfo.InvariantCulture, out totalVotes))
                    {
                        result = default;
                        return false;
                    }

                    break;

                case "victor_answer_id":

                    if (!int.TryParse(field.Value, CultureInfo.InvariantCulture, out int id))
                    {
                        result = default;
                        return false;
                    }

                    answerId = id;

                    break;

                case "victor_answer_text":
                    answerText = field.Value;
                    break;

                case "victor_answer_emoji_id":

                    if (!Snowflake.TryParse(field.Value, CultureInfo.InvariantCulture, out Snowflake emojiId))
                    {
                        result = default;
                        return false;
                    }

                    answerEmojiId = emojiId;

                    break;

                case "victor_answer_emoji_name":
                    answerEmojiName = field.Value;
                    break;

                case "victor_answer_emoji_animated":

                    if (!bool.TryParse(field.Value, out bool isAnimated))
                    {
                        result = default;
                        return false;
                    }

                    isAnswerEmojiAnimated = isAnimated;

                    break;

                default:
                    continue;
            }
        }

        result = new
        (
            questionText,
            answerVotes,
            totalVotes,
            answerId,
            answerText,
            answerEmojiId,
            answerEmojiName,
            isAnswerEmojiAnimated
        );

        return true;
    }

    /// <summary>
    /// The text of the original poll question.
    /// </summary>
    public string PollQuestionText { get; }

    /// <summary>
    /// The amount of votes cast for the winning answer.
    /// </summary>
    public int WinningAnswerVoteCount { get; }

    /// <summary>
    /// The amounts of votes cast in total, across all answers.
    /// </summary>
    public int TotalVotes { get; }

    /// <summary>
    /// The <see cref="IPollAnswer.AnswerId"/> of the winning answer.
    /// </summary>
    public int? WinningAnswerId { get; }

    /// <summary>
    /// The text of the winning answer.
    /// </summary>
    public string? WinningAnswerText { get; }

    /// <summary>
    /// The emoji ID associated with the winning answer.
    /// </summary>
    public Snowflake? WinningAnswerEmojiId { get; }

    /// <summary>
    /// The emoji name associated with the winning answer.
    /// </summary>
    public string? WinningAnswerEmojiName { get; }

    /// <summary>
    /// Indicates whether the winning answer emoji is animated, if applicable.
    /// </summary>
    public bool? IsWinningAnswerEmojiAnimated { get; }
}
