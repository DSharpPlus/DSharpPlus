using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity.Components;

/// <summary>
/// Provides a mechanism to wait for components to be interacted with and reject unwanted interactions with said components.
/// </summary>
public interface IComponentWaiter
{
    /// <summary>
    /// Waits for a component interaction matching the condition.
    /// </summary>
    public Task<Result<ComponentInteractionCreatedEventArgs>> WaitForComponentInteractionAsync
    (
        ulong messageId, 
        IReadOnlyList<string> customIds, 
        Func<ComponentInteractionCreatedEventArgs, bool> condition, 
        TimeSpan timeout
    );

    /// <summary>
    /// Handles incoming component interactions. This is called by DSharpPlus and should not be called from user code.
    /// </summary>
    /// <remarks>
    /// It is this method that handles rejecting component interactions that don't match their provided condition, but it should only do so for components that
    /// are explicitly being waited on. Interactivity should avoid responding to interactions it is not explicitly intended to handle.
    /// </remarks>
    public Task HandleComponentInteractionAsync(DiscordClient client, ComponentInteractionCreatedEventArgs eventArgs);
}
