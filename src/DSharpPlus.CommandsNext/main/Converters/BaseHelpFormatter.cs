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

using System.Collections.Generic;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext.Converters;

/// <summary>
/// Represents a base class for all default help formatters.
/// </summary>
public abstract class BaseHelpFormatter
{
    /// <summary>
    /// Gets the context in which this formatter is being invoked.
    /// </summary>
    protected CommandContext Context { get; }

    /// <summary>
    /// Gets the CommandsNext extension which constructed this help formatter.
    /// </summary>
    protected CommandsNextExtension CommandsNext => this.Context.CommandsNext;

    /// <summary>
    /// Creates a new help formatter for specified CommandsNext extension instance.
    /// </summary>
    /// <param name="ctx">Context in which this formatter is being invoked.</param>
    public BaseHelpFormatter(CommandContext ctx)
    {
        this.Context = ctx;
    }

    /// <summary>
    /// Sets the command this help message will be for.
    /// </summary>
    /// <param name="command">Command for which the help message is being produced.</param>
    /// <returns>This help formatter.</returns>
    public abstract BaseHelpFormatter WithCommand(Command command);

    /// <summary>
    /// Sets the subcommands for this command, if applicable. This method will be called with filtered data.
    /// </summary>
    /// <param name="subcommands">Subcommands for this command group.</param>
    /// <returns>This help formatter.</returns>
    public abstract BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands);

    /// <summary>
    /// Constructs the help message.
    /// </summary>
    /// <returns>Data for the help message.</returns>
    public abstract CommandHelpMessage Build();
}
