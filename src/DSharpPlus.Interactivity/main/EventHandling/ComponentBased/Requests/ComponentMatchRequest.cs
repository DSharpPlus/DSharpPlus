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
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// Represents a match that is being waited for.
/// </summary>
internal class ComponentMatchRequest
{
    /// <summary>
    /// The id to wait on. This should be uniquely formatted to avoid collisions.
    /// </summary>
    public DiscordMessage Message { get; private set; }

    /// <summary>
    /// The completion source that represents the result of the match.
    /// </summary>
    public TaskCompletionSource<ComponentInteractionCreateEventArgs> Tcs { get; private set; } = new();

    protected readonly CancellationToken _cancellation;
    protected readonly Func<ComponentInteractionCreateEventArgs, bool> _predicate;

    public ComponentMatchRequest(DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, CancellationToken cancellation)
    {
        Message = message;
        _predicate = predicate;
        _cancellation = cancellation;
        _cancellation.Register(() => Tcs.TrySetResult(null)); // TrySetCancelled would probably be better but I digress ~Velvet //
    }

    public bool IsMatch(ComponentInteractionCreateEventArgs args) => _predicate(args);
}
