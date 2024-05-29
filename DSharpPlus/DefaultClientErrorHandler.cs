using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus;

/// <summary>
/// Represents the default implementation of <see cref="IClientErrorHandler"/>
/// </summary>
public sealed class DefaultClientErrorHandler : IClientErrorHandler
{
    private readonly ILogger logger;

    /// <summary>
    /// Creates a new instance of this type.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public DefaultClientErrorHandler(ILogger<IClientErrorHandler> logger)
        => this.logger = logger;

    /// <summary>
    /// Don't use this.
    /// </summary>
    // glue code for legacy handling
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DefaultClientErrorHandler(ILogger logger)
        => this.logger = logger;

    /// <inheritdoc/>
    public ValueTask HandleEventHandlerError
    (
        string name,
        Exception exception,
        Delegate invokedDelegate,
        object sender,
        object args
    )
    {
        this.logger.LogError
        (
            exception, 
            "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaryingType})", 
            name,
            invokedDelegate.Method,
            invokedDelegate.Method.DeclaringType
        );

        return ValueTask.CompletedTask;
    }
}
