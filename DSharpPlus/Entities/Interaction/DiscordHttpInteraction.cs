using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordHttpInteraction : DiscordInteraction
{
    [JsonIgnore]
    internal readonly TaskCompletionSource taskCompletionSource = new();

    [JsonIgnore]
    internal byte[] response;

    public bool CancelHttpInteractionResponse() => this.taskCompletionSource.TrySetCanceled();

    public async Task<byte[]> GetHttpResponseAsync()
    {
        await this.taskCompletionSource.Task;

        return this.response;
    }

    /// <inheritdoc/>
    public override Task CreateResponseAsync(DiscordInteractionResponseType type, DiscordInteractionResponseBuilder? builder = null)
    {
        if (this.taskCompletionSource.Task.IsCanceled)
        {
            throw new InvalidOperationException(
                "Discord closed the connection. This is likely due to exeeding the limit of 3 seconds to the response.");
        }

        if (this.ResponseState is not DiscordInteractionResponseState.Unacknowledged)
        {
            throw new InvalidOperationException("A response has already been made to this interaction.");
        }

        this.ResponseState = type == DiscordInteractionResponseType.DeferredChannelMessageWithSource
            ? DiscordInteractionResponseState.Deferred
            : DiscordInteractionResponseState.Replied;

        DiscordInteractionResponsePayload payload = new()
        {
            Type = type,
            Data = builder is not null
                ? new DiscordInteractionApplicationCommandCallbackData
                {
                    Content = builder.Content,
                    Title = builder.Title,
                    CustomId = builder.CustomId,
                    Embeds = builder.Embeds,
                    IsTTS = builder.IsTTS,
                    Mentions = new DiscordMentions(builder.Mentions ?? Mentions.All, builder.Mentions?.Any() ?? false),
                    Flags = builder.Flags,
                    Components = builder.Components,
                    Choices = builder.Choices,
                    Poll = builder.Poll?.BuildInternal(),
                }
                : null
        };

        this.response = Encoding.UTF8.GetBytes(DiscordJson.SerializeObject(payload));
        this.taskCompletionSource.SetResult();

        return Task.CompletedTask;
    }
}
