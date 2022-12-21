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
namespace DSharpPlus;

/// <summary>
/// Denotes the type of formatting to use for timestamps.
/// </summary>
public enum TimestampFormat : byte
{
    /// <summary>
    /// A short date. e.g. 18/06/2021.
    /// </summary>
    ShortDate = (byte)'d',
    /// <summary>
    /// A long date. e.g. 18 June 2021.
    /// </summary>
    LongDate = (byte)'D',
    /// <summary>
    /// A short date and time. e.g. 18 June 2021 03:50.
    /// </summary>
    ShortDateTime = (byte)'f',
    /// <summary>
    /// A long date and time. e.g. Friday 18 June 2021 03:50.
    /// </summary>
    LongDateTime = (byte)'F',
    /// <summary>
    /// A short time. e.g. 03:50.
    /// </summary>
    ShortTime = (byte)'t',
    /// <summary>
    /// A long time. e.g. 03:50:15.
    /// </summary>
    LongTime = (byte)'T',
    /// <summary>
    /// The time relative to the client. e.g. An hour ago.
    /// </summary>
    RelativeTime = (byte)'R'
}
