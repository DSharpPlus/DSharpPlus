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

namespace DSharpPlus.CommandsNext.Exceptions
{
    /// <summary>
    /// Indicates that given command name or alias is taken.
    /// </summary>
    public class DuplicateCommandException : Exception
    {
        /// <summary>
        /// Gets the name of the command that already exists.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Creates a new exception indicating that given command name is already taken.
        /// </summary>
        /// <param name="name">Name of the command that was taken.</param>
        internal DuplicateCommandException(string name)
            : base($"A command or alias with the name '{name}' has already been registered.")
        {
            this.CommandName = name;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="DuplicateCommandException"/>.
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString() => $"{this.GetType()}: {this.Message}\nCommand name: {this.CommandName}"; // much like System.ArgumentException works
    }
}
