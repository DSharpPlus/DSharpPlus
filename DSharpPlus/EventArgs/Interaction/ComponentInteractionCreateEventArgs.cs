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
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.EventArgs
{

    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.ComponentInteractionCreated"/>.
    /// </summary>
    public class ComponentInteractionCreateEventArgs : InteractionCreateEventArgs
    {
        /// <summary>
        /// The Id of the component that was interacted with.
        /// </summary>
        public string Id => this.Interaction.Data.CustomId;

        /// <summary>
        /// The user that invoked this interaction.
        /// </summary>
        public DiscordUser User => this.Interaction.User;

        /// <summary>
        /// The guild this interaction was invoked on, if any.
        /// </summary>
        public DiscordGuild Guild => this.Channel.Guild;

        /// <summary>
        /// The channel this interaction was invoked in.
        /// </summary>
        public DiscordChannel Channel => this.Interaction.Channel;

        /// <summary>
        /// The value(s) selected. Only applicable to SelectMenu components.
        /// </summary>
        public string[] Values => this.Interaction.Data.Values;

        /// <summary>
        /// The message this interaction is attached to.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// The locale of the user that invoked this interaction.
        /// </summary>
        public string Locale => this.Interaction.Locale;

        /// <summary>
        /// The guild's locale that the user invoked in.
        /// </summary>
        public string GuildLocale => this.Interaction.GuildLocale;

        internal ComponentInteractionCreateEventArgs() { }
    }
}
