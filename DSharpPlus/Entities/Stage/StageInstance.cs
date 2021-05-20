// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using DSharpPlus.Net;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Stage instance
    /// </summary>
    public class StageInstance : SnowflakeObject, IEquatable<StageInstance>
    {
        /// <summary>
        /// Gets the guild id of the associated Stage channel
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets id of the associated Stage channel
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the topic of the Stage instance
        /// </summary>
        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; internal set; }

        #region Methods

        /// <summary>  
        /// Creates a stage instance.
        /// <param name="channel_id">Associated channel id.</param>
        /// <param name="topic">Topic of the stage instance.</param>
        /// </summary> 
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<StageInstance> OpenStageAsync(ulong channel_id, string topic)
            => await this.Discord.ApiClient.CreateStageInstaceAsync(channel_id, topic);

        /// <summary>  
        /// Gets the associated stage instance of the channel.
        /// <param name="channel_id">Associated channel id.</param>
        /// </summary> 
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<StageInstance> GetStageAsync(ulong channel_id)
            => await this.Discord.ApiClient.GetStageInstaceAsync(channel_id);

        /// <summary>
        /// Updates a stage instance.
        /// </summary>
        /// <param name="channel_id">Associated channel id.</param>
        /// <param name="topic">Topic of the stage instance.</param>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task UpdateStageAsync(ulong channel_id, string topic)
            => await this.Discord.ApiClient.ModifyStageInstaceAsync(channel_id, topic);

        /// <summary>
        /// Deletes a stage instance.
        /// </summary>
        /// <param name="channel_id">Associated channel id.</param>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task CloseStageAsync(ulong channel_id)
            => await this.Discord.ApiClient.DeleteStageInstaceAsync(channel_id);

        #endregion

        /// <summary>
        /// Checks whether this <see cref="StageInstance"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="StageInstance"/>.</returns>
        public override bool Equals(object obj) => this.Equals(obj as StageInstance);

        /// <summary>
        /// Checks whether this <see cref="StageInstance"/> is equal to another <see cref="StageInstance"/>.
        /// </summary>
        /// <param name="e"><see cref="StageInstance"/> to compare to.</param>
        /// <returns>Whether the <see cref="StageInstance"/> is equal to this <see cref="StageInstance"/>.</returns>
        public bool Equals(StageInstance e)
        {
            if (e is null)
                return false;

            return ReferenceEquals(this, e) ? true : this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="StageInstance"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="StageInstance"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="StageInstance"/> objects are equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are equal.</returns>
        public static bool operator ==(StageInstance e1, StageInstance e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            return o1 == null && o2 == null ? true : e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordChannel"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are not equal.</returns>
        public static bool operator !=(StageInstance e1, StageInstance e2)
            => !(e1 == e2);
    }
}
