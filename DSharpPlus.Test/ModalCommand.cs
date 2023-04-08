// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace DSharpPlus.Test;

public class ModalCommand : BaseCommandModule
{
    [Command]
    public async Task Modal(CommandContext ctx) => await ctx.RespondAsync(m => m.WithContent("\u200b").AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "modal", "Press for modal")));
}

[SlashCommandGroup("modal", "Slash command group for modal test commands.")]
public class ModalSlashCommands : ApplicationCommandModule
{
    [SlashCommand("user", "Modal")]
    public async Task ModalUserCommandAsync(InteractionContext ctx)
    {
        DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder()
            .WithTitle("Modal User")
            .WithCustomId("id-modal")
            .AddComponents(new TextInputComponent(label: "User", customId: "id-user", value: "id-modal", max_length: 32));
        await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);

        InteractivityExtension interactivity = ctx.Client.GetInteractivity();
        InteractivityResult<ModalSubmitEventArgs> response = await interactivity.WaitForModalAsync("id-modal", user: ctx.User, timeoutOverride: TimeSpan.FromSeconds(30));

        if (!response.TimedOut)
        {
            DiscordInteraction inter = response.Result.Interaction;
            DiscordEmbed embed = this.ModalSubmittedEmbed(ctx.User, inter, response.Result.Values);
            await inter.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        else
        {
            await ctx.Channel.SendMessageAsync("Request timed out");
        }
    }
    [SlashCommand("generic", "Modal")]
    public async Task ModalGenericCommandAsync(InteractionContext ctx)
    {
        DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder()
            .WithTitle("Modal Generic")
            .WithCustomId("id-modal")
            .AddComponents(new TextInputComponent(label: "Generic", customId: "id-generic", value: "id-modal", max_length: 32));
        await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);

        InteractivityExtension interactivity = ctx.Client.GetInteractivity();
        InteractivityResult<ModalSubmitEventArgs> response = await interactivity.WaitForModalAsync("id-modal", timeoutOverride: TimeSpan.FromSeconds(30));

        if (!response.TimedOut)
        {
            DiscordInteraction inter = response.Result.Interaction;
            DiscordEmbed embed = this.ModalSubmittedEmbed(ctx.User, inter, response.Result.Values);
            await inter.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        else
        {
            await ctx.Channel.SendMessageAsync("Request timed out");
        }
    }
    [SlashCommand("salted", "Unique modal id.")]
    public async Task ModalSaltedCommandAsync(InteractionContext ctx)
    {
        string modalId = $"id-modal-{ctx.User.Id}";
        DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder()
            .WithTitle("Modal Salted")
            .WithCustomId(modalId)
            .AddComponents(new TextInputComponent(label: "Salted", customId: "id-salted", value: modalId, max_length: 32));
        await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);

        InteractivityExtension interactivity = ctx.Client.GetInteractivity();
        InteractivityResult<ModalSubmitEventArgs> response = await interactivity.WaitForModalAsync(modalId, timeoutOverride: TimeSpan.FromSeconds(30));

        if (!response.TimedOut)
        {
            DiscordInteraction inter = response.Result.Interaction;
            DiscordEmbed embed = this.ModalSubmittedEmbed(ctx.User, inter, response.Result.Values);
            await inter.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        else
        {
            await ctx.Channel.SendMessageAsync("Request timed out");
        }
    }
    private DiscordEmbed ModalSubmittedEmbed(DiscordUser expectedUser, DiscordInteraction inter, IReadOnlyDictionary<string, string> values) => new DiscordEmbedBuilder()
            .WithAuthor(name: $"Modal Submitted: {inter.Data.CustomId}", iconUrl: inter.User.AvatarUrl)
            .WithDescription(string.Join("\n", values.Select(x => $"{x.Key}: {x.Value}")))
            .AddField("Expected", expectedUser.Mention, true).AddField("Actual", inter.User.Mention, true);
}
