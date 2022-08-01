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

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Marks this parameter as an option for a slash command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this option.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this option.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets whether this option should autocomplete.
        /// </summary>
        public bool Autocomplete { get; }

        /// <summary>
        /// Marks this parameter as an option for a slash command.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="description">The description of the option.</param>
        /// <param name="autocomplete">Whether this option should autocomplete.</param>
        public OptionAttribute(string name, string description, bool autocomplete = false)
        {
            if (name.Length > 32)
                throw new ArgumentException("Slash command option names cannot go over 32 characters.");
            if (description.Length > 100)
                throw new ArgumentException("Slash command option descriptions cannot go over 100 characters.");

            this.Name = name.ToLower();
            this.Description = description;
            this.Autocomplete = autocomplete;
        }
    }
}
