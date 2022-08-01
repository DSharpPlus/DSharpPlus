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
    /// Marks this method as a slash command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SlashCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this command.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets whether this command is enabled by default.
        /// </summary>
        public bool DefaultPermission { get; }

        /// <summary>
        /// Marks this method as a slash command.
        /// </summary>
        /// <param name="name">Sets the name of this slash command.</param>
        /// <param name="description">Sets the description of this slash command.</param>
        /// <param name="defaultPermission">Sets whether the command should be enabled by default.</param>
        public SlashCommandAttribute(string name, string description, bool defaultPermission = true)
        {
            this.Name = name.ToLower();
            this.Description = description;
            this.DefaultPermission = defaultPermission;
        }
    }
}
