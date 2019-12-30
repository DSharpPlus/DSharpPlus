using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities
{
    public class LavalinkRoutePlanner
    {
        private HttpClient Rest;

        private LavalinkConfiguration Configuration;

        private DebugLogger Logger;

        private static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

        internal LavalinkRoutePlanner(LavalinkConfiguration config, DiscordClient client)
        {
            this.Configuration = config;
            this.Logger = client.DebugLogger;

            var httphandler = new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = client.Configuration.Proxy != null
            };
            if (httphandler.UseProxy) 
                httphandler.Proxy = client.Configuration.Proxy;

            this.Rest = new HttpClient(httphandler);
            
            this.Rest.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"DSharpPlus.LavaLink/{client.VersionString}");
            this.Rest.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", config.Password);
        }

        /// <summary>
        /// Retrieves statistics from the route planner.
        /// </summary>
        /// <returns>The status (<see cref="LavalinkRouteStatus"/>) details.</returns>
        public Task<LavalinkRouteStatus> GetStatusAsync()
        {
            var routeStatusUri = new Uri($"http://{this.Configuration.RestEndpoint}/routeplanner/status");
            return this.InternalGetStatusAsync(routeStatusUri);
        }

        /// <summary>
        /// Unmarks a failed route planner IP Address.
        /// </summary>
        /// <param name="address">The IP address name to unmark.</param>
        /// <returns></returns>
        public Task FreeAddressAsync(string address)
        {
            var routeFreeAddressUri = new Uri($"http://{this.Configuration.RestEndpoint}/routeplanner/free/address");
            return this.InternalFreeAddressAsync(routeFreeAddressUri, address);
        }

        /// <summary>
        /// Unmarks all failed route planner IP Addresses.
        /// </summary>
        /// <returns></returns>
        public Task FreeAllAddressesAsync()
        {
            var routeFreeAddressUri = new Uri($"http://{this.Configuration.RestEndpoint}/routeplanner/free/all");
            return this.InternalFreeAllAddressesAsync(routeFreeAddressUri);
        }

        internal async Task<LavalinkRouteStatus> InternalGetStatusAsync(Uri uri)
        {
            using (var req = await this.Rest.GetAsync(uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, UTF8))
            {
                var json = await sr.ReadToEndAsync().ConfigureAwait(false);
                var status = JsonConvert.DeserializeObject<LavalinkRouteStatus>(json);
                return status;
            }
        }

        internal async Task InternalFreeAddressAsync(Uri uri, string address)
        {
            var payload = new StringContent(address, UTF8, "application/json");
            using (var req = await this.Rest.PostAsync(uri, payload).ConfigureAwait(false))
                if (req.StatusCode == HttpStatusCode.InternalServerError)
                    this.Logger.LogMessage(LogLevel.Warning, "Lavalink", $"Request to {uri.ToString()} returned an internal server error. This likely indicates that the server route planner configuration is incorrect.", DateTime.Now);
             
        }

        internal async Task InternalFreeAllAddressesAsync(Uri uri)
        {
            var httpReq = new HttpRequestMessage(HttpMethod.Post, uri);
            using (var req = await this.Rest.SendAsync(httpReq).ConfigureAwait(false))
                if (req.StatusCode == HttpStatusCode.InternalServerError)
                    this.Logger.LogMessage(LogLevel.Warning, "Lavalink", $"Request to {uri.ToString()} returned an internal server error. This likely indicates that the server route planner configuration is incorrect.", DateTime.Now);
        }
    }
}
