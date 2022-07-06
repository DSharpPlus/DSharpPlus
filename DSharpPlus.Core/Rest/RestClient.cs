using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using DSharpPlus.Caching.Abstractions;
using DSharpPlus.Core.Exceptions;

using Microsoft.Extensions.Options;

using Polly;
using Polly.Wrap;

namespace DSharpPlus.Core.Rest
{
    /// <summary>
    /// Represents a client for all rest requests to Discord.
    /// </summary>
    public class RestClient
    {
        private readonly ICacheService __cache;
        private readonly HttpClient __http_client;
        private readonly AsyncPolicyWrap<HttpResponseMessage>? __wrapped_policy;

        private readonly string __token;

        public RestClient(ICacheService cache, HttpClient client, IOptions<RestClientOptions> options)
        {
            __cache = cache;
            __http_client = client;
            __token = options.Value.Token;

            __wrapped_policy = Policy.WrapAsync(new PollyRatelimitPolicy(), new PollyRetryPolicy(options.Value.MaximumRetries).RetryPolicy);
        }

        public async Task<HttpResponseMessage> MakeRequestAsync(IRestRequest request)
        {
            Context requestContext = new()
            {
                ["cache"] = __cache,
                ["endpoint"] = request.Endpoint,
                ["subject-to-global-limit"] = request.IsSubjectToGlobalLimit
            };

            HttpRequestMessage requestMessage = request.Build();

            if (request.IsSubjectToGlobalLimit)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bot", __token);
            }

            requestMessage.SetPolicyExecutionContext(requestContext);

            HttpResponseMessage response = await __wrapped_policy!.ExecuteAsync(() => __http_client.SendAsync(requestMessage));

            return response.StatusCode switch
            {
                HttpStatusCode.BadRequest
                    or HttpStatusCode.MethodNotAllowed => throw new DiscordBadRequestException(requestMessage, response),

                HttpStatusCode.Unauthorized
                    or HttpStatusCode.Forbidden => throw new DiscordUnauthorizedException(requestMessage, response),

                HttpStatusCode.NotFound => throw new DiscordNotFoundException(requestMessage, response),

                HttpStatusCode.RequestEntityTooLarge => throw new DiscordPayloadTooLargeException(requestMessage, response),

                HttpStatusCode.TooManyRequests => throw new DiscordRatelimitHitException(requestMessage, response),

                HttpStatusCode.InternalServerError
                    or HttpStatusCode.BadGateway
                    or HttpStatusCode.ServiceUnavailable
                    or HttpStatusCode.GatewayTimeout => throw new DiscordServerErrorException(requestMessage, response),

                _ => response,
            };
        }
    }
}
