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

    /// <summary>
    /// The URL this request is made to. This is distinct from the <seealso cref="Route"/> in that the route
    /// cannot contain query parameters or secondary IDs necessary for the request.
    /// </summary>
    public string Url { get; init; }

    /// <summary>
    /// The ratelimiting route this request is made to.
    /// </summary>
    public string Route { get; init; }

    /// <summary>
    /// Specifies whether this request is exempt from the global limit. Generally applies to webhook requests.
    /// </summary>
    public bool IsExemptFromGlobalLimit { get; init; }

    /// <summary>
    /// Specifies whether this request is exempt from all ratelimits.
    /// </summary>
    public bool IsExemptFromAllLimits { get; init; }
}
