namespace DSharpPlus.Net;

using System.Net;

/// <summary>
/// Represents a response sent by the remote HTTP party.
/// </summary>
public record struct RestResponse
{
    /// <summary>
    /// Gets the response code sent by the remote party.
    /// </summary>
    public HttpStatusCode? ResponseCode { get; internal set; }

    /// <summary>
    /// Gets the contents of the response sent by the remote party.
    /// </summary>
    public string? Response { get; internal set; }
}
