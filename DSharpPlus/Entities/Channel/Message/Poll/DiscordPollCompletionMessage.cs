using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordPollCompletionMessage : DiscordMessage
{
    internal DiscordPollCompletionMessage(DiscordMessage other) : base(other)
    {
        Debug.Assert(other.MessageType == DiscordMessageType.PollResult);

        if (other.Embeds is not [DiscordEmbed embed])
        {
            throw new ArgumentException("The provided poll completion message had no embeds.");
        }

        if (embed.Type != "poll_result" || embed.Fields.Count == 0)
        {
            throw new ArgumentException("The provided poll completion message's embed was malformed and does not represent poll results.");
        }

        ulong winningAnswerEmojiId = 0;
        string? winningAnswerEmojiName = null;

        // https://discord.com/developers/docs/resources/message#embed-fields-by-embed-type-poll-result-embed-fields
        // i asked the devil whether they understood what the thought process here was, they said no.
        foreach (DiscordEmbedField field in embed.Fields)
        {
            switch (field.Name)
            {
                case "poll_question_text":
                    this.PollQuestionText = field.Value!;
                    break;

                case "victor_answer_votes":
                    this.WinningAnswerVoteCount = int.Parse(field.Value!);
                    break;

                case "total_votes":
                    this.TotalVotes = int.Parse(field.Value!);
                    break;

                case "victor_answer_id":
                    this.WinningAnswerId = int.Parse(field.Value!);
                    break;

                case "victor_answer_text":
                    this.WinningAnswerText = field.Value!;
                    break;

                case "victor_answer_emoji_id":
                    winningAnswerEmojiId = ulong.Parse(field.Value)!;
                    break;

                case "victor_answer_emoji_name":
                    winningAnswerEmojiName = field.Value!;
                    break;

                default:
                    continue;
            }
        }

        if (winningAnswerEmojiId != 0)
        {
            this.WinningAnswerEmoji = DiscordEmoji.FromGuildEmote(this.Discord, winningAnswerEmojiId);
        }
        else if (winningAnswerEmojiName is not null)
        {
            this.WinningAnswerEmoji = DiscordEmoji.FromUnicode(this.Discord, winningAnswerEmojiName);
        }
        else
        {
            this.WinningAnswerEmoji = null;
        }
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
    public string PollMessageLink => $"https://discord.com/channels/{this.guildId}/{this.ChannelId}/{this.Reference!.Message.Id}";
}
