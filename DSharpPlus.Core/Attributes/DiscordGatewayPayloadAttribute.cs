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
using System.Diagnostics;

namespace DSharpPlus.Core.Attributes
{
    /// <summary>
    /// Associates a payload with one or more gateway events.
    /// </summary>
    [DebuggerDisplay("Gateway Payloads: {GetDebuggerDisplay()}")]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DiscordGatewayPayloadAttribute : Attribute
    {
        /// <summary>
        /// The payloads that this record is associated with. Names should be in SCREAMING_SNAKE_CASE. See https://discord.com/developers/docs/topics/gateway#commands-and-events for possible values.
        /// </summary>
        public string[] Names { get; }

        /// <summary>
        /// The payloads that this record is associated with. Names should be in SCREAMING_SNAKE_CASE. See https://discord.com/developers/docs/topics/gateway#commands-and-events for possible values.
        /// </summary>
        /// <param name="names">The gateway event names to associate the record with.</param>
        public DiscordGatewayPayloadAttribute(params string[] names) => Names = names;

        /// <summary>
        /// Returns the names of the gateway payloads associated with this record. Not truly required, but nice to have when debugging.
        /// </summary>
        /// <returns>A `, ` delimited string of associated gateway event names.</returns>
        private string GetDebuggerDisplay() => string.Join(", ", Names);
    }
}
