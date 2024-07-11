using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;

namespace DSharpPlus.Entities;

public class DiscordHttpInteraction : DiscordInteraction
{
    internal readonly TaskCompletionSource taskCompletionSource = new();
    internal byte[] response;

    internal async Task<byte[]> GetResponseAsync()
    {
        await this.taskCompletionSource.Task;

        return this.response;
    }
    
    public override Task CreateResponseAsync(DiscordInteractionResponseType type, DiscordInteractionResponseBuilder? builder = null)
    {
        if (this.ResponseState is not DiscordInteractionResponseState.Unacknowledged)
        {
            throw new InvalidOperationException("A response has already been made to this interaction.");
        }

        this.ResponseState = type == DiscordInteractionResponseType.DeferredChannelMessageWithSource
            ? DiscordInteractionResponseState.Deferred
            : DiscordInteractionResponseState.Replied;
        
        RestInteractionResponsePayload payload = new()
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
