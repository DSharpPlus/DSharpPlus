namespace DSharpPlus.Voice.Transport.Factories;

/// <summary>
/// Provides a DI-friendly mechanism to initialize and create a transport service.
/// </summary>
public interface ITransportServiceFactory
{
    /// <summary>
    /// Creates a builder for further initialization of a transport service.
    /// </summary>
    public ITransportServiceBuilder CreateTransportService();
}
