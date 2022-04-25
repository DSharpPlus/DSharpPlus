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

using DSharpPlus.VoiceNext.Enums;

namespace DSharpPlus.VoiceNext.VoiceGatewayEntities.Commands
{
    /// <summary>
    /// To notify clients that you are speaking or have stopped speaking, send an Opcode 5 Speaking payload:
    /// </summary>
    /// <remarks>
    /// You must send at least one Opcode 5 Speaking payload before sending voice data, or you will be disconnected with an invalid SSRC error.
    /// </remarks>
    public sealed record DiscordVoiceSpeakingCommand
    {
        public DiscordVoiceSpeakingIndicators Speaking { get; internal set; }
        public int Delay { get; internal set; }
        public uint SSRC { get; internal set; }
    }
}
