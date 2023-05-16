// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using System.Threading.Tasks;

namespace DSharpPlus.AsyncEvents
{
    /// <summary>
    /// Provides a registration surface for asynchronous events using C# language event syntax. 
    /// </summary>
    /// <typeparam name="TSender">The type of the event dispatcher.</typeparam>
    /// <typeparam name="TArgs">The type of the argument object for this event.</typeparam>
    /// <param name="sender">The instance that dispatched this event.</param>
    /// <param name="args">The arguments passed to this event.</param>
    public delegate Task AsyncEventHandler<in TSender, in TArgs>
    (
        TSender sender,
        TArgs args
    )
        where TArgs : AsyncEventArgs;

    /// <summary>
    /// Provides a registration surface for a handler for exceptions raised by an async event or its registered
    /// event handlers.
    /// </summary>
    /// <typeparam name="TSender">The type of the event dispatcher.</typeparam>
    /// <typeparam name="TArgs">The type of the argument object for this event.</typeparam>
    /// <param name="event">The async event that threw this exception.</param>
    /// <param name="exception">The thrown exception.</param>
    /// <param name="handler">The async event handler that threw this exception.</param>
    /// <param name="sender">The instance that dispatched this event.</param>
    /// <param name="args">The arguments passed to this event.</param>
    public delegate void AsyncEventExceptionHandler<TSender, TArgs>
    (
        AsyncEvent<TSender, TArgs> @event,
        Exception exception,
        AsyncEventHandler<TSender, TArgs> handler,
        TSender sender,
        TArgs args
    )
        where TArgs : AsyncEventArgs;
}
