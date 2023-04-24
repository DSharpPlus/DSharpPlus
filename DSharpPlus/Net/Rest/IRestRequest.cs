using System.Net.Http;

namespace DSharpPlus.Net;

/// <summary>
/// Serves as a generic constraint for the rest client.
/// </summary>
internal interface IRestRequest
{
    /// <summary>
    /// Builds the current rest request object into a request message.
    /// </summary>
    public HttpRequestMessage Build();
}
