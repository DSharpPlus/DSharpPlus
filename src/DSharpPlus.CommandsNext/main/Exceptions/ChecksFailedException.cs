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
using System.Collections.ObjectModel;
using DSharpPlus.CommandsNext.Attributes;

namespace DSharpPlus.CommandsNext.Exceptions;

/// <summary>
/// Indicates that one or more checks for given command have failed.
/// </summary>
public class ChecksFailedException : Exception
{
    /// <summary>
    /// Gets the command that was executed.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// Gets the context in which given command was executed.
    /// </summary>
    public CommandContext Context { get; }

    /// <summary>
    /// Gets the checks that failed.
    /// </summary>
    public IReadOnlyList<CheckBaseAttribute> FailedChecks { get; }

    /// <summary>
    /// Creates a new <see cref="ChecksFailedException"/>.
    /// </summary>
    /// <param name="command">Command that failed to execute.</param>
    /// <param name="ctx">Context in which the command was executed.</param>
    /// <param name="failedChecks">A collection of checks that failed.</param>
    public ChecksFailedException(Command command, CommandContext ctx, IEnumerable<CheckBaseAttribute> failedChecks)
        : base("One or more pre-execution checks failed.")
    {
        this.Command = command;
        this.Context = ctx;
        this.FailedChecks = new ReadOnlyCollection<CheckBaseAttribute>(new List<CheckBaseAttribute>(failedChecks));
    }
}
