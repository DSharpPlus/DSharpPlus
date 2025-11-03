using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordPollCompletionMessage : DiscordMessage
{
    internal DiscordPollCompletionMessage(DiscordMessage other) : base(other)
    {
    }

    internal static DiscordPollCompletionMessage? Parse(DiscordMessage message)
    {
        Debug.Assert(message.MessageType == DiscordMessageType.PollResult);

        if (message.Embeds is not [DiscordEmbed embed])
        {
            message.Discord.Logger.LogInformation(
                "Received a poll completion message without an embed. This is likely due to missing the MessageContent intent.");
            return null;
        }

        DiscordPollCompletionMessage result = new(message);

        if (embed.Type != "poll_result" || embed.Fields.Count == 0)
        {
            throw new ArgumentException(
                "The provided poll completion message's embed was malformed and does not represent poll results.");
        }

        ulong winningAnswerEmojiId = 0;
        string? winningAnswerEmojiName = null;
        bool winningAnswerEmojiIsAnimated = false;

        // https://discord.com/developers/docs/resources/message#embed-fields-by-embed-type-poll-result-embed-fields
        // i asked the devil whether they understood what the thought process here was, they said no.
        foreach (DiscordEmbedField field in embed.Fields)
        {
            switch (field.Name)
            {
                case "poll_question_text":
                    result.PollQuestionText = field.Value!;
                    break;

                case "victor_answer_votes":
                    result.WinningAnswerVoteCount = int.Parse(field.Value!);
                    break;

                case "total_votes":
                    result.TotalVotes = int.Parse(field.Value!);
                    break;

                case "victor_answer_id":
                    result.WinningAnswerId = int.Parse(field.Value!);
                    break;

                case "victor_answer_text":
                    result.WinningAnswerText = field.Value!;
                    break;

                case "victor_answer_emoji_id":
                    winningAnswerEmojiId = ulong.Parse(field.Value!);
                    break;

                case "victor_answer_emoji_name":
                    winningAnswerEmojiName = field.Value!;
                    break;

                case "victor_answer_emoji_animated":
                    winningAnswerEmojiIsAnimated = bool.TryParse(field.Value!, out bool animated) && animated;
                    break;

                default:
                    continue;
            }
        }

        if (winningAnswerEmojiId != 0)
        {
            result.WinningAnswerEmoji = new DiscordEmoji
            {
                Id = winningAnswerEmojiId,
                Name = winningAnswerEmojiName ?? "",
                IsAnimated = winningAnswerEmojiIsAnimated,
                IsManaged = false,
                Discord = result.Discord
            };
        }
        else if (winningAnswerEmojiName is not null)
        {
            result.WinningAnswerEmoji = DiscordEmoji.FromUnicode(result.Discord, winningAnswerEmojiName);
        }
        else
        {
            result.WinningAnswerEmoji = null;
        }

        return result;
    }

    /// <summary>
    /// The text of the original poll question.
    /// </summary>
    [JsonIgnore]
    public string PollQuestionText { get; private set; }

    /// <summary>
    /// Indicates whether the poll ended in a draw
    /// </summary>
    [JsonIgnore]
    public bool IsDraw => this.WinningAnswerId is null;

    /// <summary>
    /// The amount of votes cast for the winning answer.
    /// </summary>
    [JsonIgnore]
    public int WinningAnswerVoteCount { get; private set; }

    /// <summary>
    /// The amounts of votes cast in total, across all answers.
    /// </summary>
    [JsonIgnore]
    public int TotalVotes { get; private set; }

    /// <summary>
    /// The <see cref="DiscordPollAnswer.AnswerId"/> of the winning answer.
    /// </summary>
    [JsonIgnore]
    public int? WinningAnswerId { get; private set; }

    /// <summary>
    /// The text of the winning answer.
    /// </summary>
    [JsonIgnore]
    public string? WinningAnswerText { get; private set; }

    /// <summary>
    /// The emoji associated with the winning answer.
    /// </summary>
    [JsonIgnore]
    public DiscordEmoji? WinningAnswerEmoji { get; private set; }

    /// <summary>
    /// The original poll message.
    /// </summary>
    [JsonIgnore]
    public DiscordMessage? PollMessage => this.Reference!.Message;

    /// <summary>
    /// A message link pointing to the poll message.
    /// </summary>
    [JsonIgnore]
    public string PollMessageLink =>
        $"https://discord.com/channels/{this.guildId}/{this.ChannelId}/{this.Reference!.Message.Id}";
}
