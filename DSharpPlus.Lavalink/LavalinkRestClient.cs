// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.Exceptions;
using DSharpPlus.Net;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Lavalink
{
    /// <summary>
    /// Represents a class for Lavalink REST calls.
    /// </summary>
    public sealed class LavalinkRestClient
    {
        /// <summary>
        /// Gets the REST connection endpoint for this client.
        /// </summary>
        public ConnectionEndpoint RestEndpoint { get; private set; }

        private HttpClient _http;

        private readonly ILogger _logger;

        private readonly Lazy<string> _dsharpplusVersionString = new(() =>
        {
            var a = typeof(DiscordClient).GetTypeInfo().Assembly;

            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
                return iv.InformationalVersion;

            var v = a.GetName().Version;
            var vs = v.ToString(3);

            if (v.Revision > 0)
                vs = $"{vs}, CI build {v.Revision}";

            return vs;
        });

        /// <summary>
        /// Creates a new Lavalink REST client.
        /// </summary>
        /// <param name="restEndpoint">The REST server endpoint to connect to.</param>
        /// <param name="password">The password for the remote server.</param>
        public LavalinkRestClient(ConnectionEndpoint restEndpoint, string password)
        {
            this.RestEndpoint = restEndpoint;
            this.ConfigureHttpHandling(password);
        }

        internal LavalinkRestClient(LavalinkConfiguration config, BaseDiscordClient client)
        {
            this.RestEndpoint = config.RestEndpoint;
            this._logger = client.Logger;
            this.ConfigureHttpHandling(config.Password, client);
        }

        /// <summary>
        /// Gets the version of the Lavalink server.
        /// </summary>
        /// <returns></returns>
        public Task<string> GetVersionAsync()
        {
            var versionUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.VERSION}");
            return this.InternalGetVersionAsync(versionUri);
        }

        #region Track_Loading

        /// <summary>
        /// Searches for specified terms.
        /// </summary>
        /// <param name="searchQuery">What to search for.</param>
        /// <param name="type">What platform will search for.</param>
        /// <returns>A collection of tracks matching the criteria.</returns>
        public Task<LavalinkLoadResult> GetTracksAsync(string searchQuery, LavalinkSearchType type = LavalinkSearchType.Youtube)
        {
            var prefix = type switch
            {
                LavalinkSearchType.Youtube => "ytsearch:",
                LavalinkSearchType.SoundCloud => "scsearch:",
                LavalinkSearchType.Plain => "",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            var str = WebUtility.UrlEncode(prefix + searchQuery);
            var tracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.LOAD_TRACKS}?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }

        /// <summary>
        /// Loads tracks from specified URL.
        /// </summary>
        /// <param name="uri">URL to load tracks from.</param>
        /// <returns>A collection of tracks from the URL.</returns>
        public Task<LavalinkLoadResult> GetTracksAsync(Uri uri)
        {
            var str = WebUtility.UrlEncode(uri.AbsoluteUri);
            var tracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.LOAD_TRACKS}?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }

        /// <summary>
        /// Loads tracks from a local file.
        /// </summary>
        /// <param name="file">File to load tracks from.</param>
        /// <returns>A collection of tracks from the file.</returns>
        public Task<LavalinkLoadResult> GetTracksAsync(FileInfo file)
        {
            var str = WebUtility.UrlEncode(file.FullName);
            var tracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.LOAD_TRACKS}?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }

        /// <summary>
        /// Decodes a base64 track string into a Lavalink track object.
        /// </summary>
        /// <param name="trackString">The base64 track string.</param>
        /// <returns></returns>
        public Task<LavalinkTrack> DecodeTrackAsync(string trackString)
        {
            var str = WebUtility.UrlEncode(trackString);
            var decodeTrackUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.DECODE_TRACK}?track={str}");
            return this.InternalDecodeTrackAsync(decodeTrackUri);
        }

        /// <summary>
        /// Decodes an array of base64 track strings into Lavalink track objects.
        /// </summary>
        /// <param name="trackStrings">The array of base64 track strings.</param>
        /// <returns></returns>
        public Task<IEnumerable<LavalinkTrack>> DecodeTracksAsync(string[] trackStrings)
        {
            var decodeTracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.DECODE_TRACKS}");
            return this.InternalDecodeTracksAsync(decodeTracksUri, trackStrings);
        }

        /// <summary>
        /// Decodes a list of base64 track strings into Lavalink track objects.
        /// </summary>
        /// <param name="trackStrings">The list of base64 track strings.</param>
        /// <returns></returns>
        public Task<IEnumerable<LavalinkTrack>> DecodeTracksAsync(List<string> trackStrings)
        {
            var decodeTracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.DECODE_TRACKS}");
            return this.InternalDecodeTracksAsync(decodeTracksUri, trackStrings.ToArray());
        }

        #endregion

        #region Route_Planner

        /// <summary>
        /// Retrieves statistics from the route planner.
        /// </summary>
        /// <returns>The status (<see cref="LavalinkRouteStatus"/>) details.</returns>
        public Task<LavalinkRouteStatus> GetRoutePlannerStatusAsync()
        {
            var routeStatusUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.ROUTE_PLANNER}{Endpoints.STATUS}");
            return this.InternalGetRoutePlannerStatusAsync(routeStatusUri);
        }

        /// <summary>
        /// Unmarks a failed route planner IP Address.
        /// </summary>
        /// <param name="address">The IP address name to unmark.</param>
        /// <returns></returns>
        public Task FreeAddressAsync(string address)
        {
            var routeFreeAddressUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.ROUTE_PLANNER}{Endpoints.FREE_ADDRESS}");
            return this.InternalFreeAddressAsync(routeFreeAddressUri, address);
        }

        /// <summary>
        /// Unmarks all failed route planner IP Addresses.
        /// </summary>
        /// <returns></returns>
        public Task FreeAllAddressesAsync()
        {
            var routeFreeAllAddressesUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.ROUTE_PLANNER}{Endpoints.FREE_ALL}");
            return this.InternalFreeAllAddressesAsync(routeFreeAllAddressesUri);
        }

        #endregion

        internal async Task<string> InternalGetVersionAsync(Uri uri)
        {
            using var req = await this._http.GetAsync(uri).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.UTF8);
            var json = await sr.ReadToEndAsync().ConfigureAwait(false);
            return json;
        }

        #region Internal_Track_Loading

        internal async Task<LavalinkLoadResult> InternalResolveTracksAsync(Uri uri)
        {
            using var req = await this._http.GetAsync(uri).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.UTF8);
            var json = await sr.ReadToEndAsync().ConfigureAwait(false);
            if (!req.IsSuccessStatusCode)
            {
                var jsonError = JObject.Parse(json);
                throw new TrackLoadException($"Unable to load tracks: {jsonError["message"]}");
            }

            var load = JsonConvert.DeserializeObject<LavalinkLoadResult>(json);

            if (load.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                throw new TrackLoadException($"Unable to load tracks: {load.Exception.Message}");
            }

            return load;
        }

        internal async Task<LavalinkTrack> InternalDecodeTrackAsync(Uri uri)
        {
            using var req = await this._http.GetAsync(uri).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.UTF8);
            var json = await sr.ReadToEndAsync().ConfigureAwait(false);
            if (!req.IsSuccessStatusCode)
            {
                var jsonError = JObject.Parse(json);
                this._logger?.LogError(LavalinkEvents.LavalinkDecodeError, "Unable to decode track strings: {0}", jsonError["message"]);

                return null;
            }
            var track = JsonConvert.DeserializeObject<LavalinkTrack>(json);
            return track;
        }

        internal async Task<IEnumerable<LavalinkTrack>> InternalDecodeTracksAsync(Uri uri, string[] ids)
        {
            var jsonOut = JsonConvert.SerializeObject(ids);
            var content = new StringContent(jsonOut, Utilities.UTF8, "application/json");
            using var req = await this._http.PostAsync(uri, content).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.UTF8);
            var jsonIn = await sr.ReadToEndAsync().ConfigureAwait(false);
            if (!req.IsSuccessStatusCode)
            {
                var jsonError = JObject.Parse(jsonIn);
                this._logger?.LogError(LavalinkEvents.LavalinkDecodeError, "Unable to decode track strings", jsonError["message"]);
                return null;
            }

            var jarr = JToken.Parse(jsonIn) as JArray;
            var decodedTracks = new LavalinkTrack[jarr.Count];

            for (var i = 0; i < decodedTracks.Length; i++)
            {
                decodedTracks[i] = JsonConvert.DeserializeObject<LavalinkTrack>(jarr[i]["info"].ToString());
                decodedTracks[i].Encoded = jarr[i]["track"].ToString();
            }

            var decodedTrackList = new ReadOnlyCollection<LavalinkTrack>(decodedTracks);

            return decodedTrackList;
        }

        #endregion

        #region Internal_Route_Planner

        internal async Task<LavalinkRouteStatus> InternalGetRoutePlannerStatusAsync(Uri uri)
        {
            using var req = await this._http.GetAsync(uri).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.UTF8);
            var json = await sr.ReadToEndAsync().ConfigureAwait(false);
            var status = JsonConvert.DeserializeObject<LavalinkRouteStatus>(json);
            return status;
        }

        internal async Task InternalFreeAddressAsync(Uri uri, string address)
        {
            var payload = new StringContent(address, Utilities.UTF8, "application/json");
            using var req = await this._http.PostAsync(uri, payload).ConfigureAwait(false);
            if (req.StatusCode == HttpStatusCode.InternalServerError)
                this._logger?.LogWarning(LavalinkEvents.LavalinkRestError, "Request to {0} returned an internal server error - your server route planner configuration is likely incorrect", uri);

        }

        internal async Task InternalFreeAllAddressesAsync(Uri uri)
        {
            var httpReq = new HttpRequestMessage(HttpMethod.Post, uri);
            using var req = await this._http.SendAsync(httpReq).ConfigureAwait(false);
            if (req.StatusCode == HttpStatusCode.InternalServerError)
                this._logger?.LogWarning(LavalinkEvents.LavalinkRestError, "Request to {0} returned an internal server error - your server route planner configuration is likely incorrect", uri);
        }

        #endregion

        #region Player

        internal async Task<LavalinkPlayer> UpdatePlayerAsync(ulong guildId, string sessionId, LavalinkPlayerUpdatePayload updatePayload, bool noReplace = false)
        {
            var payload = JsonConvert.SerializeObject(updatePayload);
            var content = new StringContent(payload, Utilities.UTF8, "application/json");
            var message = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(string.Format(Endpoints.PLAYER, this.RestEndpoint.ToHttpString(), sessionId, guildId)))
            {
                Content = content
            };
            using var req = await this._http.SendAsync(message).ConfigureAwait(false);
            var res = await req.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!req.IsSuccessStatusCode)
            {
                var jo = JObject.Parse(res);
                throw new HttpRequestException(jo["message"].ToString());
            }

            return JsonConvert.DeserializeObject<LavalinkPlayer>(res);
        }

        public async Task<LavalinkPlayer> GetPlayerAsync(string sessionId, ulong guildId)
        {
            using var req = await this._http.GetAsync(new Uri(string.Format(Endpoints.PLAYER, this.RestEndpoint.ToHttpString(), sessionId, guildId))).ConfigureAwait(false);
            var res = await req.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!req.IsSuccessStatusCode)
            {
                var jo = JObject.Parse(res);
                throw new HttpRequestException(jo["message"].ToString());
            }

            return JsonConvert.DeserializeObject<LavalinkPlayer>(res);
        }

        #endregion
        private void ConfigureHttpHandling(string password, BaseDiscordClient client = null)
        {
            var httphandler = new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = client != null && client.Configuration.Proxy != null
            };
            if (httphandler.UseProxy) // because mono doesn't implement this properly
                httphandler.Proxy = client.Configuration.Proxy;

            this._http = new HttpClient(httphandler);

            this._http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"DSharpPlus.LavaLink/{this._dsharpplusVersionString}");
            this._http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", password);
        }


    }
}
