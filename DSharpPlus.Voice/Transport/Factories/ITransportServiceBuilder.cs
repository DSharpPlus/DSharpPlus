namespace DSharpPlus.Voice.Transport.Factories;

/// <summary>
/// An abstraction for creating a transport service.
/// </summary>
public interface ITransportServiceBuilder
{
    /// <summary>
    /// Creates a TransportServiceBuilder instance
    /// </summary>
    /// <returns></returns>
    public IInitializedTransportServiceBuilder CreateBuilder();
}
