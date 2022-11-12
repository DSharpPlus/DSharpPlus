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
using System.IO;
using System.Text;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Interface that provides abstractions for the various message builder types in DSharpPlus,
    /// allowing re-use of code.
    /// </summary>
    public interface IDiscordMessageBuilder<T> where T : IDiscordMessageBuilder<T>
        // This has got to be the most big brain thing I have ever done with interfaces lmfao
    {
        string Content { get; set; }

        bool IsTTS { get; set; }

        IReadOnlyList<DiscordEmbed> Embeds { get; }

        IReadOnlyList<DiscordMessageFile> Files { get; }

        IReadOnlyList<DiscordActionRowComponent> Components { get; }

        IReadOnlyList<IMention> Mentions { get; }

        T WithContent(string content);

        T AddComponents(params DiscordComponent[] components);

        T AddComponents(IEnumerable<DiscordComponent> components);

        T AddComponents(IEnumerable<DiscordActionRowComponent> components);

        T WithTTS(bool isTTS);

        T AddEmbed(DiscordEmbed embed);

        T AddEmbeds(IEnumerable<DiscordEmbed> embeds);

        T AddFile(string fileName, Stream stream, bool resetStream = false);

        T AddFile(FileStream stream, bool resetStream = false);

        T AddFiles(IDictionary<string, Stream> files, bool resetStreams = false);

        T AddMention(IMention mention);

        T AddMentions(IEnumerable<IMention> mentions);

        void ClearComponents();

        void Clear();
    }
}
