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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSharpPlus.Core.Enums
{
    /// <summary>
    /// Embed types are "loosely defined" and, for the most part, are not used by our clients for rendering. Embed attributes power what is rendered.
    /// </summary>
    /// <remarks>
    /// Embed types should be considered deprecated and might be removed in a future API version.
    /// </remarks>
    [JsonConverter(typeof(StringEnumConverter)), Obsolete("Embed types are \"loosely defined\" and, for the most part, are not used by our clients for rendering. Embed attributes power what is rendered. Embed types should be considered deprecated and might be removed in a future API version.")]
    public enum DiscordEmbedType
    {
        Rich,
        Image,
        Video,
        Gifv,
        Article,
        Link
    }
}
