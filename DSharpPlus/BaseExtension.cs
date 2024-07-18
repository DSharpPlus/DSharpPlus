using System;

namespace DSharpPlus;

/// <summary>
/// Represents base for all DSharpPlus extensions. To implement your own extension, extend this class, and implement its abstract members.
/// </summary>
public abstract class BaseExtension : IDisposable
{
    /// <summary>
    /// Gets the instance of <see cref="DiscordClient"/> this extension is attached to.
    /// </summary>
    public DiscordClient Client { get; protected set; }

    public abstract void Dispose();

    /// <summary>
    /// Initializes this extension for given <see cref="DiscordClient"/> instance.
    /// </summary>
    /// <param name="client">Discord client to initialize for.</param>
    public abstract void Setup(DiscordClient client);
}
