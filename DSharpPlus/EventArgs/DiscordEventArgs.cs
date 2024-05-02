using DSharpPlus.AsyncEvents;

namespace DSharpPlus.EventArgs;

// Note: this might seem useless, but should we ever need to add a common property or method to all event arg
// classes, it would be useful to already have a base for all of it.

/// <summary>
/// Common base for all other <see cref="DiscordClient"/>-related event argument classes.
/// </summary>
public abstract class DiscordEventArgs : AsyncEventArgs
{
    protected DiscordEventArgs()
    { }
}
