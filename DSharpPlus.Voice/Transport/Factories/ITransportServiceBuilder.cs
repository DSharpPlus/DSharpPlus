namespace DSharpPlus.Voice.Transport.Factories;

/// <summary>
/// An abstraction for creating a transport service.
/// </summary>
public interface ITransportServiceBuilder
{
    public IInitializedTransportServiceBuilder CreateBuilder();
}
