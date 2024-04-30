namespace DSharpPlus.Lavalink;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Net;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        Assembly a = typeof(DiscordClient).GetTypeInfo().Assembly;

        AssemblyInformationalVersionAttribute? iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (iv != null)
        {
            return iv.InformationalVersion;
        }

        Version? v = a.GetName().Version;
        string vs = v.ToString(3);

        if (v.Revision > 0)
        {
            vs = $"{vs}, CI build {v.Revision}";
        }

        return vs;
    });

    /// <summary>
    /// Creates a new Lavalink REST client.
    /// </summary>
    /// <param name="restEndpoint">The REST server endpoint to connect to.</param>
    /// <param name="password">The password for the remote server.</param>
    public LavalinkRestClient(ConnectionEndpoint restEndpoint, string password)
    {
        RestEndpoint = restEndpoint;
        ConfigureHttpHandling(password);
    }

    internal LavalinkRestClient(LavalinkConfiguration config, BaseDiscordClient client)
    {
        RestEndpoint = config.RestEndpoint;
        _logger = client.Logger;
        ConfigureHttpHandling(config.Password, client);
    }

    /// <summary>
    /// Gets the version of the Lavalink server.
    /// </summary>
    /// <returns></returns>
    public Task<string> GetVersionAsync()
    {
        Uri versionUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.VERSION}");
        return InternalGetVersionAsync(versionUri);
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
        string prefix = type switch
        {
            LavalinkSearchType.Youtube => "ytsearch:",
            LavalinkSearchType.SoundCloud => "scsearch:",
            LavalinkSearchType.Plain => "",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        string str = WebUtility.UrlEncode(prefix + searchQuery);
        Uri tracksUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.LOAD_TRACKS}?identifier={str}");
        return InternalResolveTracksAsync(tracksUri);
    }

    /// <summary>
    /// Loads tracks from specified URL.
    /// </summary>
    /// <param name="uri">URL to load tracks from.</param>
    /// <returns>A collection of tracks from the URL.</returns>
    public Task<LavalinkLoadResult> GetTracksAsync(Uri uri)
    {
        string str = WebUtility.UrlEncode(uri.AbsoluteUri);
        Uri tracksUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.LOAD_TRACKS}?identifier={str}");
        return InternalResolveTracksAsync(tracksUri);
    }

    /// <summary>
    /// Loads tracks from a local file.
    /// </summary>
    /// <param name="file">File to load tracks from.</param>
    /// <returns>A collection of tracks from the file.</returns>
    public Task<LavalinkLoadResult> GetTracksAsync(FileInfo file)
    {
        string str = WebUtility.UrlEncode(file.FullName);
        Uri tracksUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.LOAD_TRACKS}?identifier={str}");
        return InternalResolveTracksAsync(tracksUri);
    }

    /// <summary>
    /// Decodes a base64 track string into a Lavalink track object.
    /// </summary>
    /// <param name="trackString">The base64 track string.</param>
    /// <returns></returns>
    public Task<LavalinkTrack> DecodeTrackAsync(string trackString)
    {
        string str = WebUtility.UrlEncode(trackString);
        Uri decodeTrackUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.DECODE_TRACK}?track={str}");
        return InternalDecodeTrackAsync(decodeTrackUri);
    }

    /// <summary>
    /// Decodes an array of base64 track strings into Lavalink track objects.
    /// </summary>
    /// <param name="trackStrings">The array of base64 track strings.</param>
    /// <returns></returns>
    public Task<IEnumerable<LavalinkTrack>> DecodeTracksAsync(string[] trackStrings)
    {
        Uri decodeTracksUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.DECODE_TRACKS}");
        return InternalDecodeTracksAsync(decodeTracksUri, trackStrings);
    }

    /// <summary>
    /// Decodes a list of base64 track strings into Lavalink track objects.
    /// </summary>
    /// <param name="trackStrings">The list of base64 track strings.</param>
    /// <returns></returns>
    public Task<IEnumerable<LavalinkTrack>> DecodeTracksAsync(List<string> trackStrings)
    {
        Uri decodeTracksUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.DECODE_TRACKS}");
        return InternalDecodeTracksAsync(decodeTracksUri, trackStrings.ToArray());
    }

    #endregion

    #region Route_Planner

    /// <summary>
    /// Retrieves statistics from the route planner.
    /// </summary>
    /// <returns>The status (<see cref="LavalinkRouteStatus"/>) details.</returns>
    public Task<LavalinkRouteStatus> GetRoutePlannerStatusAsync()
    {
        Uri routeStatusUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.ROUTE_PLANNER}{Endpoints.STATUS}");
        return InternalGetRoutePlannerStatusAsync(routeStatusUri);
    }

    /// <summary>
    /// Unmarks a failed route planner IP Address.
    /// </summary>
    /// <param name="address">The IP address name to unmark.</param>
    /// <returns></returns>
    public Task FreeAddressAsync(string address)
    {
        Uri routeFreeAddressUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.ROUTE_PLANNER}{Endpoints.FREE_ADDRESS}");
        return InternalFreeAddressAsync(routeFreeAddressUri, address);
    }

    /// <summary>
    /// Unmarks all failed route planner IP Addresses.
    /// </summary>
    /// <returns></returns>
    public Task FreeAllAddressesAsync()
    {
        Uri routeFreeAllAddressesUri = new Uri($"{RestEndpoint.ToHttpString()}{Endpoints.ROUTE_PLANNER}{Endpoints.FREE_ALL}");
        return InternalFreeAllAddressesAsync(routeFreeAllAddressesUri);
    }

    #endregion

    internal async Task<string> InternalGetVersionAsync(Uri uri)
    {
        using HttpResponseMessage req = await _http.GetAsync(uri);
        using Stream res = await req.Content.ReadAsStreamAsync();
        using StreamReader sr = new StreamReader(res, Utilities.UTF8);
        string json = await sr.ReadToEndAsync();
        return json;
    }

    #region Internal_Track_Loading

    internal async Task<LavalinkLoadResult> InternalResolveTracksAsync(Uri uri)
    {
        // this function returns a Lavalink 3-like dataset regardless of input data version

        string json = "[]";
        using (HttpResponseMessage req = await _http.GetAsync(uri))
        using (Stream res = await req.Content.ReadAsStreamAsync())
        using (StreamReader sr = new StreamReader(res, Utilities.UTF8))
        {
            json = await sr.ReadToEndAsync();
        }

        JToken jdata = JToken.Parse(json);
        if (jdata is JArray jarr)
        {
            // Lavalink 2.x

            List<LavalinkTrack> tracks = new List<LavalinkTrack>(jarr.Count);
            foreach (JToken jt in jarr)
            {
                LavalinkTrack track = jt["info"].ToDiscordObject<LavalinkTrack>();
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
            LavalinkLoadResult loadInfo = jo.ToDiscordObject<LavalinkLoadResult>();
            List<LavalinkTrack> tracks = new List<LavalinkTrack>(jarr.Count);
            foreach (JToken jt in jarr)
            {
                LavalinkTrack track = jt["info"].ToDiscordObject<LavalinkTrack>();
                track.TrackString = jt["track"].ToString();

                tracks.Add(track);
            }

            loadInfo.Tracks = new ReadOnlyCollection<LavalinkTrack>(tracks);

            return loadInfo;
        }
        else
        {
            return null;
        }
    }

    internal async Task<LavalinkTrack> InternalDecodeTrackAsync(Uri uri)
    {
        using HttpResponseMessage req = await _http.GetAsync(uri);
        using Stream res = await req.Content.ReadAsStreamAsync();
        using StreamReader sr = new StreamReader(res, Utilities.UTF8);
        string json = await sr.ReadToEndAsync();
        if (!req.IsSuccessStatusCode)
        {
            JObject jsonError = JObject.Parse(json);
            _logger?.LogError(LavalinkEvents.LavalinkDecodeError, "Unable to decode track strings: {0}", jsonError["message"]);

            return null;
        }
        LavalinkTrack? track = JsonConvert.DeserializeObject<LavalinkTrack>(json);
        return track;
    }

    internal async Task<IEnumerable<LavalinkTrack>> InternalDecodeTracksAsync(Uri uri, string[] ids)
    {
        string jsonOut = JsonConvert.SerializeObject(ids);
        StringContent content = new StringContent(jsonOut, Utilities.UTF8, "application/json");
        using HttpResponseMessage req = await _http.PostAsync(uri, content);
        using Stream res = await req.Content.ReadAsStreamAsync();
        using StreamReader sr = new StreamReader(res, Utilities.UTF8);
        string jsonIn = await sr.ReadToEndAsync();
        if (!req.IsSuccessStatusCode)
        {
            JObject jsonError = JObject.Parse(jsonIn);
            _logger?.LogError(LavalinkEvents.LavalinkDecodeError, "Unable to decode track strings", jsonError["message"]);
            return null;
        }

        JArray? jarr = JToken.Parse(jsonIn) as JArray;
        LavalinkTrack[] decodedTracks = new LavalinkTrack[jarr.Count];

        for (int i = 0; i < decodedTracks.Length; i++)
        {
            decodedTracks[i] = JsonConvert.DeserializeObject<LavalinkTrack>(jarr[i]["info"].ToString());
            decodedTracks[i].TrackString = jarr[i]["track"].ToString();
        }

        ReadOnlyCollection<LavalinkTrack> decodedTrackList = new ReadOnlyCollection<LavalinkTrack>(decodedTracks);

        return decodedTrackList;
    }

    #endregion

    #region Internal_Route_Planner

    internal async Task<LavalinkRouteStatus> InternalGetRoutePlannerStatusAsync(Uri uri)
    {
        using HttpResponseMessage req = await _http.GetAsync(uri);
        using Stream res = await req.Content.ReadAsStreamAsync();
        using StreamReader sr = new StreamReader(res, Utilities.UTF8);
        string json = await sr.ReadToEndAsync();
        LavalinkRouteStatus? status = JsonConvert.DeserializeObject<LavalinkRouteStatus>(json);
        return status;
    }

    internal async Task InternalFreeAddressAsync(Uri uri, string address)
    {
        StringContent payload = new StringContent(address, Utilities.UTF8, "application/json");
        using HttpResponseMessage req = await _http.PostAsync(uri, payload);
        if (req.StatusCode == HttpStatusCode.InternalServerError)
        {
            _logger?.LogWarning(LavalinkEvents.LavalinkRestError, "Request to {0} returned an internal server error - your server route planner configuration is likely incorrect", uri);
        }
    }

    internal async Task InternalFreeAllAddressesAsync(Uri uri)
    {
        HttpRequestMessage httpReq = new HttpRequestMessage(HttpMethod.Post, uri);
        using HttpResponseMessage req = await _http.SendAsync(httpReq);
        if (req.StatusCode == HttpStatusCode.InternalServerError)
        {
            _logger?.LogWarning(LavalinkEvents.LavalinkRestError, "Request to {0} returned an internal server error - your server route planner configuration is likely incorrect", uri);
        }
    }

    #endregion

    private void ConfigureHttpHandling(string password, BaseDiscordClient client = null)
    {
        HttpClientHandler httphandler = new HttpClientHandler
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            UseProxy = client != null && client.Configuration.Proxy != null
        };
        if (httphandler.UseProxy) // because mono doesn't implement this properly
        {
            httphandler.Proxy = client.Configuration.Proxy;
        }

        _http = new HttpClient(httphandler);

        _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"DSharpPlus.LavaLink/{_dsharpplusVersionString}");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", password);
    }
}
