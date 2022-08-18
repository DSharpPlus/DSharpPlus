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
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    public sealed class ContextMenuInteractionCreateEventArgs : InteractionCreateEventArgs
    {
        /// <summary>
        /// The type of context menu that was used. This is never <see cref="ApplicationCommandType.SlashCommand"/>.
        /// </summary>
        public ApplicationCommandType Type { get; internal set; } //TODO: Set this

        /// <summary>
        /// The user that invoked this interaction. Can be casted to a member if this was on a guild.
        /// </summary>
        public DiscordUser User => this.Interaction.User;

        /// <summary>
        /// The member that invoked this interaction. <see langword="null"/> if this was on a DM.
        /// </summary>
        public DiscordMember Member => this.Interaction.Member;

        /// <summary>
        /// The user this interaction targets, if applicable.
        /// </summary>
        public DiscordUser TargetUser { get; internal set; }

        /// <summary>
        /// The member this interaction targets, if applicable.
        /// </summary>
        public DiscordMember TargetMember => this.TargetUser as DiscordMember;

        /// <summary>
        /// The message this interaction targets, if applicable.
        /// </summary>
        public DiscordMessage TargetMessage { get; internal set; }
    }
}
