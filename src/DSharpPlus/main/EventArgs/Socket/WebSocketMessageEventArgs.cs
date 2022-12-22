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

using Emzi0767.Utilities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents base class for raw socket message event arguments.
/// </summary>
public abstract class SocketMessageEventArgs : AsyncEventArgs
{ }

/// <summary>
/// Represents arguments for text message websocket event.
/// </summary>
public sealed class SocketTextMessageEventArgs : SocketMessageEventArgs
{
    /// <summary>
    /// Gets the received message string.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a new instance of text message event arguments.
    /// </summary>
    /// <param name="message">Received message string.</param>
    public SocketTextMessageEventArgs(string message) => Message = message;
}

/// <summary>
/// Represents arguments for binary message websocket event.
/// </summary>
public sealed class SocketBinaryMessageEventArgs : SocketMessageEventArgs
{
    /// <summary>
    /// Gets the received message bytes.
    /// </summary>
    public byte[] Message { get; }

    /// <summary>
    /// Creates a new instance of binary message event arguments.
    /// </summary>
    /// <param name="message">Received message bytes.</param>
    public SocketBinaryMessageEventArgs(byte[] message) => Message = message;
}
