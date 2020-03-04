﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Lavalink.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Lavalink
{
    public sealed class LavalinkRest
    {
        private HttpClient HttpClient;

        private LavalinkConfiguration Configuration;

        private DebugLogger Logger;

        internal LavalinkRest(LavalinkConfiguration config, DiscordClient client)
        {
            this.Configuration = config;
            this.Logger = client.DebugLogger;

            var httphandler = new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = client.Configuration.Proxy != null
            };
            if (httphandler.UseProxy) // because mono doesn't implement this properly
                httphandler.Proxy = client.Configuration.Proxy;

            this.HttpClient = new HttpClient(httphandler);

            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"DSharpPlus.LavaLink/{client.VersionString}");
            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", config.Password);
        }

        /// <summary>
        /// Gets the version of the Lavalink server.
        /// </summary>
        /// <returns></returns>
        public Task<string> GetVersionAsync()
        {
            var versionUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.VERSION}");
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
            string prefix;
            if (type == LavalinkSearchType.Youtube)
                prefix = "ytsearch";
            else
                prefix = "scsearch";

            var str = WebUtility.UrlEncode($"{prefix}:{searchQuery}");
            var tracksUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.LOAD_TRACKS}?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }

        /// <summary>
        /// Loads tracks from specified URL.
        /// </summary>
        /// <param name="uri">URL to load tracks from.</param>
        /// <returns>A collection of tracks from the URL.</returns>
        public Task<LavalinkLoadResult> GetTracksAsync(Uri uri)
        {
            var str = WebUtility.UrlEncode(uri.ToString());
            var tracksUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.LOAD_TRACKS}?identifier={str}");
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
            var tracksUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.LOAD_TRACKS}?identifier={str}");
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
            var decodeTrackUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.DECODE_TRACK}?track={str}");
            return this.InternalDecodeTrackAsync(decodeTrackUri);
        }

        /// <summary>
        /// Decodes an array of base64 track strings into Lavalink track objects.
        /// </summary>
        /// <param name="trackStrings">The array of base64 track strings.</param>
        /// <returns></returns>
        public Task<IEnumerable<LavalinkTrack>> DecodeTracksAsync(string[] trackStrings)
        {
            var decodeTracksUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.DECODE_TRACKS}");
            return this.InternalDecodeTracksAsync(decodeTracksUri, trackStrings);
        }

        /// <summary>
        /// Decodes a list of base64 track strings into Lavalink track objects.
        /// </summary>
        /// <param name="trackStrings">The list of base64 track strings.</param>
        /// <returns></returns>
        public Task<IEnumerable<LavalinkTrack>> DecodeTracksAsync(List<string> trackStrings)
        {
            var decodeTracksUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.DECODE_TRACKS}");
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
            var routeStatusUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.ROUTE_PLANNER}{Endpoints.STATUS}");
            return this.InternalGetRoutePlannerStatusAsync(routeStatusUri);
        }

        /// <summary>
        /// Unmarks a failed route planner IP Address.
        /// </summary>
        /// <param name="address">The IP address name to unmark.</param>
        /// <returns></returns>
        public Task FreeAddressAsync(string address)
        {
            var routeFreeAddressUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.ROUTE_PLANNER}{Endpoints.FREE_ADDRESS}");
            return this.InternalFreeAddressAsync(routeFreeAddressUri, address);
        }

        /// <summary>
        /// Unmarks all failed route planner IP Addresses.
        /// </summary>
        /// <returns></returns>
        public Task FreeAllAddressesAsync()
        {
            var routeFreeAllAddressesUri = new Uri($"http://{this.Configuration.RestEndpoint}{Endpoints.ROUTE_PLANNER}{Endpoints.FREE_ALL}");
            return this.InternalFreeAllAddressesAsync(routeFreeAllAddressesUri);
        }

        #endregion

        internal async Task<string> InternalGetVersionAsync(Uri uri)
        {
            using (var req = await this.HttpClient.GetAsync(uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Utilities.UTF8))
            {
                var json = await sr.ReadToEndAsync().ConfigureAwait(false);
                return json;
            }           
        }

        #region Internal_Track_Loading

        internal async Task<LavalinkLoadResult> InternalResolveTracksAsync(Uri uri)
        {
            // this function returns a Lavalink 3-like dataset regardless of input data version

            var json = "[]";
            using (var req = await this.HttpClient.GetAsync(uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Utilities.UTF8))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var jdata = JToken.Parse(json);
            if (jdata is JArray jarr)
            {
                // Lavalink 2.x

                var tracks = new List<LavalinkTrack>(jarr.Count);
                foreach (var jt in jarr)
                {
                    var track = jt["info"].ToObject<LavalinkTrack>();
                    track.TrackString = jt["track"].ToString();

                    tracks.Add(track);
                }

                return new LavalinkLoadResult
                {
                    PlaylistInfo = default,
                    LoadResultType = tracks.Count == 0 ? LavalinkLoadResultType.LoadFailed : LavalinkLoadResultType.TrackLoaded,
                    Tracks = tracks
                };
            }
            else if (jdata is JObject jo)
            {
                // Lavalink 3.x

                jarr = jo["tracks"] as JArray;
                var loadInfo = jo.ToObject<LavalinkLoadResult>();
                var tracks = new List<LavalinkTrack>(jarr.Count);
                foreach (var jt in jarr)
                {
                    var track = jt["info"].ToObject<LavalinkTrack>();
                    track.TrackString = jt["track"].ToString();

                    tracks.Add(track);
                }

                loadInfo.Tracks = new ReadOnlyCollection<LavalinkTrack>(tracks);

                return loadInfo;
            }
            else
                return null;
        }

        internal async Task<LavalinkTrack> InternalDecodeTrackAsync(Uri uri)
        {
            using (var req = await this.HttpClient.GetAsync(uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Utilities.UTF8))
            {
                var json = await sr.ReadToEndAsync().ConfigureAwait(false);
                if (!req.IsSuccessStatusCode)
                {
                    var jsonError = JToken.Parse(json) as JObject;
                    this.Logger.LogMessage(LogLevel.Error, "Lavalink", $"Unable to decode the given track strings: {jsonError["message"]}", DateTime.Now);
                    return null;
                }
                var track = JsonConvert.DeserializeObject<LavalinkTrack>(json);
                return track;
            }
        }

        internal async Task<IEnumerable<LavalinkTrack>> InternalDecodeTracksAsync(Uri uri, string[] ids)
        {
            var jsonOut = JsonConvert.SerializeObject(ids);
            var content = new StringContent(jsonOut, Utilities.UTF8, "application/json");
            using (var req = await this.HttpClient.PostAsync(uri, content).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Utilities.UTF8))
            {
                var jsonIn = await sr.ReadToEndAsync().ConfigureAwait(false);
                if (!req.IsSuccessStatusCode)
                {
                    var jsonError = JToken.Parse(jsonIn) as JObject;
                    this.Logger.LogMessage(LogLevel.Error, "Lavalink", $"Unable to decode the given track strings: {jsonError["message"]}", DateTime.Now);
                    return null;
                }

                var jarr = JToken.Parse(jsonIn) as JArray;
                var decodedTracks = new LavalinkTrack[jarr.Count];

                for (var i = 0; i < decodedTracks.Length; i++)
                {
                    decodedTracks[i] = JsonConvert.DeserializeObject<LavalinkTrack>(jarr[i]["info"].ToString());
                    decodedTracks[i].TrackString = jarr[i]["track"].ToString();
                }

                var decodedTrackList = new ReadOnlyCollection<LavalinkTrack>(decodedTracks);

                return decodedTrackList;
            }
        }

        #endregion

        #region Internal_Route_Planner

        internal async Task<LavalinkRouteStatus> InternalGetRoutePlannerStatusAsync(Uri uri)
        {
            using (var req = await this.HttpClient.GetAsync(uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Utilities.UTF8))
            {
                var json = await sr.ReadToEndAsync().ConfigureAwait(false);
                var status = JsonConvert.DeserializeObject<LavalinkRouteStatus>(json);
                return status;
            }
        }

        internal async Task InternalFreeAddressAsync(Uri uri, string address)
        {
            var payload = new StringContent(address, Utilities.UTF8, "application/json");
            using (var req = await this.HttpClient.PostAsync(uri, payload).ConfigureAwait(false))
                if (req.StatusCode == HttpStatusCode.InternalServerError)
                    this.Logger.LogMessage(LogLevel.Warning, "Lavalink", $"Request to {uri.ToString()} returned an internal server error. This likely indicates that the server route planner configuration is incorrect.", DateTime.Now);

        }

        internal async Task InternalFreeAllAddressesAsync(Uri uri)
        {
            var httpReq = new HttpRequestMessage(HttpMethod.Post, uri);
            using (var req = await this.HttpClient.SendAsync(httpReq).ConfigureAwait(false))
                if (req.StatusCode == HttpStatusCode.InternalServerError)
                    this.Logger.LogMessage(LogLevel.Warning, "Lavalink", $"Request to {uri.ToString()} returned an internal server error. This likely indicates that the server route planner configuration is incorrect.", DateTime.Now);
        }

        #endregion
    }
}
