using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Transport;

/// <inheritdoc cref="ITransportServiceFactory"/>
public sealed class TransportServiceFactory : ITransportServiceFactory
{
    private readonly ILogger<ITransportService> logger;

    public TransportServiceFactory(ILogger<ITransportService> logger)
        => this.logger = logger;

    /// <inheritdoc/>
    public ITransportServiceBuilder CreateTransportService()
        => new TransportServiceBuilder(this.logger);
}
