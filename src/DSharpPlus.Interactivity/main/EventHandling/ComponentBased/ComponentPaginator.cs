// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling;

internal class ComponentPaginator : IPaginator
{
    private readonly DiscordClient _client;
    private readonly InteractivityConfiguration _config;
    private readonly DiscordMessageBuilder _builder = new();
    private readonly Dictionary<ulong, IPaginationRequest> _requests = new();

    public ComponentPaginator(DiscordClient client, InteractivityConfiguration config)
    {
        _client = client;
        _client.ComponentInteractionCreated += Handle;
        _config = config;
    }

    public async Task DoPaginationAsync(IPaginationRequest request)
    {
        ulong id = (await request.GetMessageAsync().ConfigureAwait(false)).Id;
        _requests.Add(id, request);

        try
        {
            TaskCompletionSource<bool> tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
            await tcs.Task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "There was an exception while paginating.");
        }
        finally
        {
            _requests.Remove(id);
            try
            {
                await request.DoCleanupAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "There was an exception while cleaning up pagination.");
            }
        }
    }

    public void Dispose() => _client.ComponentInteractionCreated -= Handle;


    private async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs e)
    {
        if (!_requests.TryGetValue(e.Message.Id, out IPaginationRequest? req))
        {
            return;
        }

        if (_config.AckPaginationButtons)
        {
            e.Handled = true;
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
        }

        if (await req.GetUserAsync().ConfigureAwait(false) != e.User)
        {
            if (_config.ResponseBehavior is InteractionResponseBehavior.Respond)
            {
                await e.Interaction.CreateFollowupMessageAsync(new() { Content = _config.ResponseMessage, IsEphemeral = true }).ConfigureAwait(false);
            }

            return;
        }

        if (req is InteractionPaginationRequest ipr)
        {
            ipr.RegenerateCTS(e.Interaction); // Necessary to ensure we don't prematurely yeet the CTS //
        }

        await HandlePaginationAsync(req, e).ConfigureAwait(false);
    }

    private async Task HandlePaginationAsync(IPaginationRequest request, ComponentInteractionCreateEventArgs args)
    {
        PaginationButtons buttons = _config.PaginationButtons;
        DiscordMessage msg = await request.GetMessageAsync().ConfigureAwait(false);
        string id = args.Id;
        TaskCompletionSource<bool> tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);

        Task paginationTask = id switch
        {
            _ when id == buttons.SkipLeft.CustomId => request.SkipLeftAsync(),
            _ when id == buttons.SkipRight.CustomId => request.SkipRightAsync(),
            _ when id == buttons.Stop.CustomId => Task.FromResult(tcs.TrySetResult(true)),
            _ when id == buttons.Left.CustomId => request.PreviousPageAsync(),
            _ when id == buttons.Right.CustomId => request.NextPageAsync(),
            _ => Task.CompletedTask
        };

        await paginationTask.ConfigureAwait(false);

        if (id == buttons.Stop.CustomId)
        {
            return;
        }

        Page page = await request.GetPageAsync().ConfigureAwait(false);
        IEnumerable<DiscordButtonComponent> bts = await request.GetButtonsAsync().ConfigureAwait(false);

        if (request is InteractionPaginationRequest ipr)
        {
            DiscordWebhookBuilder builder = new DiscordWebhookBuilder()
                .WithContent(page.Content)
                .AddEmbed(page.Embed)
                .AddComponents(bts);

            await args.Interaction.EditOriginalResponseAsync(builder).ConfigureAwait(false);
            return;
        }


        _builder.Clear();

        _builder
            .WithContent(page.Content)
            .AddEmbed(page.Embed)
            .AddComponents(bts);

        await _builder.ModifyAsync(msg).ConfigureAwait(false);

    }
}
