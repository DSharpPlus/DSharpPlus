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

using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Converters
{
    // A direct copy of DiscordMemberArgumentConverter.cs
    public sealed class DiscordUserArgumentConverter : ISlashArgumentConverter<DiscordUser>
    {
        public async Task<Optional<DiscordUser>> ConvertAsync(InteractionContext interactionContext, DiscordInteractionDataOption interactionDataOption, ParameterInfo interactionMethodArgument)
        {
            // I don't know when this'll ever fail, but I've added other checks just in case.
            if (interactionContext.Interaction.Data.Resolved.Users != null && interactionContext.Interaction.Data.Resolved.Users.TryGetValue((ulong)interactionDataOption.Value, out var user))
            {
                return Optional.FromValue(user);
            }
            else if (interactionContext.Client.UserCache.TryGetValue((ulong)interactionDataOption.Value, out user))
            {
                return Optional.FromValue(user);
            }
            else
            {
                // CNext makes the API request, attempting to replicate behavior.
                return Optional.FromValue(await interactionContext.Client.GetUserAsync((ulong)interactionDataOption.Value).ConfigureAwait(false));
            }
        }
    }
}
