using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;

using DSharpPlus.Net.HttpInteractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace DSharpPlus.HttpInteractions.AspNetCore;

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

    private static async Task HandleDiscordInteractionAsync(HttpContext httpContext, CancellationToken cancellationToken, [FromServices] DiscordClient client)
    {
        if (!httpContext.Request.Headers.TryGetValue(HeaderNames.ContentLength, out StringValues lengthString) 
            || !int.TryParse(lengthString, out int length))
        {
            httpContext.Response.StatusCode = 400;
            return;
        }
                
        byte[] bodyBuffer = ArrayPool<byte>.Shared.Rent(length);
        await httpContext.Request.Body.ReadExactlyAsync(bodyBuffer.AsMemory(..length));
                
        if (!TryExtractHeaders(httpContext.Request.Headers, out string? timestamp, out string? key))
        {
            httpContext.Response.StatusCode = 401;
            return;
        }

        if (!DiscordHeaders.VerifySignature(bodyBuffer.AsSpan(..length), timestamp!, key!, client.CurrentApplication.VerifyKey))
        {
            httpContext.Response.StatusCode = 401;
            return;
        }
                
        byte[] result = await client.HandleHttpInteractionAsync(bodyBuffer[..length], cancellationToken);
                
        ArrayPool<byte>.Shared.Return(bodyBuffer);
                
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentLength = result.Length;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
                
        await httpContext.Response.Body.WriteAsync(result);
    }
    

    public static bool TryExtractHeaders(IDictionary<string, StringValues> headers, out string? timestamp, out string? key)
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
