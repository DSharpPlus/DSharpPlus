using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mime;

using DSharpPlus.Net.InboundWebhooks;
using DSharpPlus.Net.InboundWebhooks.Transport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace DSharpPlus.Http.AspNetCore;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Registers an endpoint to handle HTTP-based interactions from Discord
    /// </summary>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder AddDiscordHttpInteractions
    (
        this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string url = "/interactions"
    ) 
        => builder.MapPost(url, HandleDiscordInteractionAsync);

    private static async Task HandleDiscordInteractionAsync
    (
        HttpContext httpContext,
        CancellationToken cancellationToken,

        [FromServices]
        DiscordClient client,

        [FromServices] 
        IInteractionTransportService transportService
    )
    {
        (int length, byte[]? bodyBuffer) = await ExtractAndValidateBodyAsync(httpContext, cancellationToken, client);

        if (length == -1 || bodyBuffer == null)
        {
            return;
        }

        ArraySegment<byte> body = new(bodyBuffer, 0, length);

        byte[] result = await transportService.HandleHttpInteractionAsync(body, cancellationToken);

        ArrayPool<byte>.Shared.Return(bodyBuffer);

        httpContext.Response.StatusCode = (int) HttpStatusCode.OK;
        httpContext.Response.ContentLength = result.Length;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        await httpContext.Response.Body.WriteAsync(result, cancellationToken);
    }
    
    /// <summary>
    /// Registers an endpoint to handle HTTP-based events from Discord
    /// </summary>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder AddDiscordWebhookEvents
    (
        this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string url = "/webhook-events"
    ) 
        => builder.MapPost(url, HandleDiscordWebhookEventAsync);
    
    private static async Task HandleDiscordWebhookEventAsync
    (
        HttpContext httpContext,
        CancellationToken cancellationToken,

        [FromServices]
        DiscordClient client,

        [FromServices] 
        IWebhookTransportService transportService
    )
    {
        (int length, byte[]? bodyBuffer) = await ExtractAndValidateBodyAsync(httpContext, cancellationToken, client);

        if (length == -1 || bodyBuffer == null)
        {
            return;
        }

        ArraySegment<byte> body = new(bodyBuffer, 0, length);

        
        // ReSharper disable MethodSupportsCancellation => we dont care if the request was canceld and always want to return the buffer
        _ = transportService.HandleWebhookEventAsync(body).ContinueWith(x => ArrayPool<byte>.Shared.Return(bodyBuffer));
        // ReSharper restore MethodSupportsCancellation
        
        httpContext.Response.StatusCode = (int) HttpStatusCode.NoContent;
    }

    private static async Task<(int length, byte[]? bodyBuffer)> ExtractAndValidateBodyAsync
    (
        HttpContext httpContext,
        CancellationToken cancellationToken,
        DiscordClient client
    )
    {
        if (!httpContext.Request.Headers.TryGetValue(HeaderNames.ContentLength, out StringValues lengthString) 
            || !int.TryParse(lengthString, out int length))
        {
            httpContext.Response.StatusCode = 400;
            return (-1, null);
        }

        byte[] bodyBuffer = ArrayPool<byte>.Shared.Rent(length);
        await httpContext.Request.Body.ReadExactlyAsync(bodyBuffer.AsMemory(..length), cancellationToken);

        if (!TryExtractHeaders(httpContext.Request.Headers, out string? timestamp, out string? key))
        {
            httpContext.Response.StatusCode = 401;
            ArrayPool<byte>.Shared.Return(bodyBuffer);
            return (-1, null);
        }

        if (!DiscordHeaders.VerifySignature(bodyBuffer.AsSpan(..length), timestamp!, key!, client.CurrentApplication.VerifyKey))
        {
            httpContext.Response.StatusCode = 401;
            ArrayPool<byte>.Shared.Return(bodyBuffer);
            return (-1, null);
        }

        return (length, bodyBuffer);
    }
    
    internal static bool TryExtractHeaders(IDictionary<string, StringValues> headers, out string? timestamp, out string? key)
    {
        timestamp = null;
        key = null;
        if (headers.TryGetValue(DiscordHeaders.TimestampHeaderName, out StringValues svTimestamp))
        {
            timestamp = svTimestamp;
        }

        if (headers.TryGetValue(DiscordHeaders.SignatureHeaderName, out StringValues svKey))
        {
            key = svKey;
        }

        return timestamp is not null && key is not null;
    }
}
