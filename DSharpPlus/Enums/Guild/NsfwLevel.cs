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
    /// Represents a server's conetent level.
    /// </summary>
    public enum NsfwLevel
    {
        //I'm going off a hunch; default = safe(?) who knows. //
        /// <summary>
        /// Indicates a server's nsfw level is the default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Indicates a server's content contains explicit material.
        /// </summary>
        Explicit = 1,
        /// <summary>
        /// Indicates a server's content is safe for work (SFW).
        /// </summary>
        Safe = 2,
        /// <summary>
        /// Indicates a server's content is age-gated.
        /// </summary>
        Age_Restricted = 3
    }
}
