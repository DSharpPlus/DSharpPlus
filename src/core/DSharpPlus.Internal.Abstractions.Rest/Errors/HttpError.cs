// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Net;

using DSharpPlus.Results.Errors;

namespace DSharpPlus.Internal.Abstractions.Rest.Errors;

/// <summary>
/// Represents a HTTP error returned by an API call.
/// </summary>
public sealed record HttpError : Error
{
    /// <summary>
    /// The encountered status code.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// Initializes a new HttpError from the provided status code and message.
    /// </summary>
    /// <param name="statusCode">The HTTP status encountered.</param>
    /// <param name="message">
    /// The error message, either a synthesized human-readable error or the error string returned
    /// by Discord, which may be parsed programmatically.
    /// </param>
    public HttpError
    (
        HttpStatusCode statusCode,
        string? message = null
    )
        : base(message ?? $"Encountered HTTP status code {(ulong)statusCode}: {statusCode}")
        => this.StatusCode = statusCode;
}
