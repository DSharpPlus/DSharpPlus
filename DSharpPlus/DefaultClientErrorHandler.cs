using System;
using System.ComponentModel;
using System.Threading.Tasks;

using DSharpPlus.Exceptions;

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
        if (exception is BadRequestException badRequest)
        {
            this.logger.LogError
            (
                "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaryingType}):\n" +
                "A request was rejected by the Discord API.\n" +
                "  Errors: {Errors}\n" +
                "  Message: {JsonMessage}\n" +
                "  Stack trace: {Stacktrace}",
                name,
                invokedDelegate.Method,
                invokedDelegate.Method.DeclaringType,
                badRequest.Errors,
                badRequest.JsonMessage,
                badRequest.StackTrace
            );

            return ValueTask.CompletedTask;
        }

        this.logger.LogError
        (
            exception, 
            "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaryingType}).", 
            name,
            invokedDelegate.Method,
            invokedDelegate.Method.DeclaringType
        );

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask HandleGatewayError(Exception exception)
    {
        this.logger.LogError
        (
            exception,
            "An error occurred in the DSharpPlus gateway."
        );

        return ValueTask.CompletedTask;
    }
}
