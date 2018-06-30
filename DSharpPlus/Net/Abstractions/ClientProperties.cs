using System;
using System.Reflection;
#if !HAS_ENVIRONMENT
using System.Runtime.InteropServices;
#endif
using Newtonsoft.Json;
#if WINDOWS_UWP
using Windows.System.Profile;
#endif

namespace DSharpPlus.Net.Abstractions
{
    /// <summary>
    /// Represents data for identify payload's client properties.
    /// </summary>
    internal sealed class ClientProperties
    {
        /// <summary>
        /// Gets the client's operating system.
        /// </summary>
        [JsonProperty("$os")]
        public string OperatingSystem =>
#if WINDOWS_UWP
            "uap10";
#else
            $"{Environment.OSVersion.VersionString}";
#endif

        /// <summary>
        /// Gets the client's browser.
        /// </summary>
        [JsonProperty("$browser")]
        public string Browser =>
#if WINDOWS_UWP
            "DiscordUWP";
#else
            "DiscordWPF";
#endif

        /// <summary>
        /// Gets the client's device.
        /// </summary>
        [JsonProperty("$device")]
        public string Device =>
#if WINDOWS_UWP
            "{AnalyticsInfo.VersionInfo.DeviceFamily} ({AnalyticsInfo.DeviceForm})";
#else
            "";
#endif

        /// <summary>
        /// Gets the client's referrer.
        /// </summary>
        [JsonProperty("$referrer")]
        public string Referrer
            => "";

        /// <summary>
        /// Gets the client's referring domain.
        /// </summary>
        [JsonProperty("$referring_domain")]
        public string ReferringDomain
            => "";
    }
}
