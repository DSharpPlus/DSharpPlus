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
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Models
{
    public class ScheduledGuildEventEditModel : BaseEditModel
    {
        /// <summary>
        /// The new name of the event.
        /// </summary>
        public Optional<string> Name { get; set; }

        /// <summary>
        /// The new description of the event.
        /// </summary>
        public Optional<string> Description { get; set; }

        /// <summary>
        /// The new channel ID of the event. This must be set to null for external events.
        /// </summary>
        public Optional<DiscordChannel?> Channel { get; set; }

        /// <summary>
        /// The new privacy of the event.
        /// </summary>
        public Optional<ScheduledGuildEventPrivacyLevel> PrivacyLevel { get; set; }

        /// <summary>
        /// The type of the event.
        /// </summary>
        public Optional<ScheduledGuildEventType> Type { get; set; }

        /// <summary>
        /// The new time of the event.
        /// </summary>
        public Optional<DateTimeOffset> StartTime { get; set; }

        /// <summary>
        /// The new end time of the event.
        /// </summary>
        public Optional<DateTimeOffset> EndTime { get; set; }

        /// <summary>
        /// The new metadata of the event.
        /// </summary>
        public Optional<DiscordScheduledGuildEventMetadata> Metadata { get; set; }

        /// <summary>
        /// The new status of the event.
        /// </summary>
        public Optional<ScheduledGuildEventStatus> Status { get; set; }

        internal ScheduledGuildEventEditModel() { }

    }
}
