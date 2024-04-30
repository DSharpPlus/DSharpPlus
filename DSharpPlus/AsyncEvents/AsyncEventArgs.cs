namespace DSharpPlus.AsyncEvents;
using System;

/// <summary>
/// A base class for arguments passed to an event handler.
/// </summary>
public class AsyncEventArgs : System.EventArgs
{
    /// <summary>
    /// [UNUSED] This used to set whether an event was handled.
    /// </summary>
    // true here causes a compiler error, making this a source breaking change
    // no matter whether warnings as errors is enabled or not. this should ensure
    // that no one uses this any more.
    [Obsolete("This is no longer utilized in DSharpPlus.", true)]
    public bool Handled { get; set; } = false;
}
