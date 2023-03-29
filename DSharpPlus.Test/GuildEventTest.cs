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
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace DSharpPlus.Test;

public class GuildEventTest : BaseCommandModule
{
    [Command("create_event")]
    public async Task CreateEventAsync(CommandContext ctx, string name, string location = null, [RemainingText] string description = null)
    {
        try
        {
            await ctx.Guild.CreateEventAsync(name, description, null, ScheduledGuildEventType.External, ScheduledGuildEventPrivacyLevel.GuildOnly, DateTimeOffset.Now + TimeSpan.FromMinutes(10), DateTimeOffset.Now + TimeSpan.FromMinutes(15), location);
            await ctx.RespondAsync("Event created!");
        }
        catch (BadRequestException ex)
        {
            await ctx.RespondAsync(ex.JsonMessage);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync(ex.Message);
        }
    }
}
