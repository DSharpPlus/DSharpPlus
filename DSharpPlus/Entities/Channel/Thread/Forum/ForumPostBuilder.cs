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
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.Entities
{
    public class ForumPostBuilder
    {
        public string Name { get; set; }
        public int? SlowMode { get; set; }
        public DiscordMessageBuilder Message { get; set; }
        public IReadOnlyList<DiscordForumTag> AppliedTags { get; }
        public AutoArchiveDuration? AutoArchiveDuration { get; set; }

        public ForumPostBuilder()
        {
           AppliedTags = new List<DiscordForumTag>();
        }

        public ForumPostBuilder WithName(string name)
        {
            this.Name = name;
            return this;
        }

        public ForumPostBuilder WithSlowMode(int slowMode)
        {
            this.SlowMode = slowMode;
            return this;
        }

        public ForumPostBuilder WithSlowMode(TimeSpan slowMode)
        {
            this.SlowMode = (int)slowMode.TotalSeconds;
            return this;
        }

        public ForumPostBuilder WithMessage(DiscordMessageBuilder message)
        {
            this.Message = message;
            return this;
        }

        public ForumPostBuilder WithAutoArchiveDuration(AutoArchiveDuration autoArchiveDuration)
        {
            this.AutoArchiveDuration = autoArchiveDuration;
            return this;
        }

        public ForumPostBuilder AddTag(DiscordForumTag tag)
        {
            ((List<DiscordForumTag>)this.AppliedTags).Add(tag);
            return this;
        }

        public ForumPostBuilder AddTags(IEnumerable<DiscordForumTag> tags)
        {
            ((List<DiscordForumTag>)this.AppliedTags).AddRange(tags);
            return this;
        }

        public ForumPostBuilder RemoveTag(DiscordForumTag tag)
        {
            ((List<DiscordForumTag>)this.AppliedTags).Remove(tag);
            return this;
        }
    }
}
