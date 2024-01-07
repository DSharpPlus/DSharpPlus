// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA2227

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Rest;

/// <summary>
/// Provides an user-friendly way to build a rest request to Discord.
/// </summary>
public class RequestBuilder
{
    /// <summary>
    /// The audit log reason for Discord, if applicable.
    /// </summary>
    public string? AuditLogReason { get; set; }

    /// <summary>
    /// The payload to send to discord.
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// The route this request will take.
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Additional information to pass to the rest client.
    /// </summary>
    public IDictionary<string, object>? AdditionalContext { get; set; }

    /// <summary>
    /// Additional files to upload with this request.
    /// </summary>
    public IDictionary<string, AttachmentData>? AdditionalFiles { get; set; }

    /// <summary>
    /// Additional headers to add to this request.
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Specifies the audit log reason for this request.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public RequestBuilder WithAuditLogReason(string reason)
    {
        this.AuditLogReason = reason;
        return this;
    }

    /// <summary>
    /// Attaches a payload to this request.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public RequestBuilder WithPayload(object payload)
    {
        this.Payload = payload;
        return this;
    }

    /// <summary>
    /// Specifies the route this request will take.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public RequestBuilder WithRoute(string route)
    {
        this.Route = route;
        return this;
    }

    /// <summary>
    /// Adds a field to the request context.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public RequestBuilder AddToContext(string key, object value)
    {
        this.AdditionalContext ??= new Dictionary<string, object>();

        this.AdditionalContext.Add(key, value);
        return this;
    }

    /// <summary>
    /// Adds a file to attach to the request.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public RequestBuilder AddFile(string name, AttachmentData file)
    {
        this.AdditionalFiles ??= new Dictionary<string, AttachmentData>();

        this.AdditionalFiles.Add(name, file);
        return this;
    }

    /// <summary>
    /// Adds a header to the request.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public RequestBuilder AddHeader(string key, string value)
    {
        this.Headers ??= new Dictionary<string, string>();

        this.Headers.Add(key, value);
        return this;
    }
}
