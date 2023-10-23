using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DSharpPlus.Net;

/// <summary>
/// Represents a non-multipart HTTP request.
/// </summary>
internal readonly record struct RestRequest : IRestRequest
{
    /// <inheritdoc/>
    public string Url { get; init; }

    /// <summary>
    /// The method for this request.
    /// </summary>
    public HttpMethod Method { get; init; }

    /// <inheritdoc/>
    public string Route { get; init; }

    /// <inheritdoc/>
    public bool IsExemptFromGlobalLimit { get; init; }

    /// <summary>
    /// The headers for this request.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// The payload sent with this request.
    /// </summary>
    public string? Payload { get; init; }

    /// <inheritdoc/>
    public HttpRequestMessage Build()
    {
        HttpRequestMessage request = new()
        {
            Method = this.Method,
            RequestUri = new($"{Endpoints.BASE_URI}/{this.Url}")
        };

        if (this.Payload is not null)
        {
            request.Content = new StringContent(this.Payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        }

        if (this.Headers is not null)
        {
            foreach (KeyValuePair<string, string> header in this.Headers)
            {
                request.Headers.Add(header.Key, Uri.EscapeDataString(header.Value));
            }
        }

        return request;
    }
}
