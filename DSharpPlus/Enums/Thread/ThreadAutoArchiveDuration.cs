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

namespace DSharpPlus
{
    /// <summary>
    /// Represents the auto-archive duration for a thread.
    /// </summary>
    public enum ThreadAutoArchiveDuration : int
    {
        /// <summary>
        /// Indicates that the thread will be auto archived after one hour.
        /// </summary>
        OneHour = 60,

        /// <summary>
        /// Indicates that the thread will be auto archived after one day / twentyfour hours.
        /// </summary>
        OneDay = 1440,

        /// <summary>
        /// Indicates that the thread will be auto archived after three days. Needs boost level 1 for guild.
        /// </summary>
        ThreeDays = 4320,

        /// <summary>
        /// Indicates that the thread will be auto archived after a week. Needs boost level 2 for guild.
        /// </summary>
        OneWeek = 10080
    }
}
