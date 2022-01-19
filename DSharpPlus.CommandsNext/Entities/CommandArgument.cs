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

namespace DSharpPlus.CommandsNext
{
    public sealed class CommandArgument
    {
        /// <summary>
        /// Gets this argument's name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets this argument's type.
        /// </summary>
        public Type Type { get; internal set; }

        /// <summary>
        /// Gets or sets whether this argument is an array argument.
        /// </summary>
        internal bool IsArray { get; set; } = false;

        /// <summary>
        /// Gets whether this argument is optional.
        /// </summary>
        public bool IsOptional { get; internal set; }

        /// <summary>
        /// Gets this argument's default value.
        /// </summary>
        public object DefaultValue { get; internal set; }

        /// <summary>
        /// Gets whether this argument catches all remaining arguments.
        /// </summary>
        public bool IsCatchAll { get; internal set; }

        /// <summary>
        /// Gets this argument's description.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the custom attributes attached to this argument.
        /// </summary>
        public IReadOnlyCollection<Attribute> CustomAttributes { get; internal set; }
    }
}
