// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Rest.Ratelimiting;

namespace DSharpPlus.Internal.Rest;

internal static class RequestBuilderExtensions
{
    public static RequestBuilder WithSimpleRoute(this RequestBuilder request, ISimpleRatelimitRoute route)
    {
        request.AddToContext("simple-route", route);
        return request;
    }

    public static RequestBuilder WithFullRatelimit(this RequestBuilder request, string route)
    {
        request.AddToContext("full-ratelimit", route);
        return request;
    }

    public static RequestBuilder AsWebhookRequest(this RequestBuilder request, bool isWebhookRequest = true)
    {
        request.AddToContext("is-webhook-request", isWebhookRequest);
        return request;
    }

    public static RequestBuilder AsInteractionRequest(this RequestBuilder request, bool isInteractionRequest = true)
    {
        request.AddToContext("is-interaction-request", isInteractionRequest);
        return request;
    }
}
