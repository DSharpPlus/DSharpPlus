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

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity;

/// <summary>
/// Contains well-defined event IDs used by the Interactivity extension.
/// </summary>
public static class InteractivityEvents
{
    /// <summary>
    /// Miscellaneous events, that do not fit in any other category.
    /// </summary>
    public static EventId Misc { get; } = new EventId(500, "Interactivity");

    /// <summary>
    /// Events pertaining to errors that happen during waiting for events.
    /// </summary>
    public static EventId InteractivityWaitError { get; } = new EventId(501, nameof(InteractivityWaitError));

    /// <summary>
    /// Events pertaining to pagination.
    /// </summary>
    public static EventId InteractivityPaginationError { get; } = new EventId(502, nameof(InteractivityPaginationError));

    /// <summary>
    /// Events pertaining to polling.
    /// </summary>
    public static EventId InteractivityPollError { get; } = new EventId(503, nameof(InteractivityPollError));

    /// <summary>
    /// Events pertaining to event collection.
    /// </summary>
    public static EventId InteractivityCollectorError { get; } = new EventId(504, nameof(InteractivityCollectorError));
}
