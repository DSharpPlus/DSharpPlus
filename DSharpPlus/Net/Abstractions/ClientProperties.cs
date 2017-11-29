﻿using System;
using System.Reflection;
#if !HAS_ENVIRONMENT
using System.Runtime.InteropServices;
#endif
using Newtonsoft.Json;

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
        public string OperatingSystem
        {
            get
            {
#if !HAS_ENVIRONMENT
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "windows";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "linux";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return "osx";

                var plat = RuntimeInformation.OSDescription.ToLowerInvariant();
#else
                if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.WinCE)
                    return "windows";
                else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                    return "osx";
                else if (Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.Xbox)
                    return "potato";
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                    return "unix";

                var plat = Environment.OSVersion.VersionString;
#endif
                if (plat.Contains("freebsd"))
                    return "freebsd";
                else if (plat.Contains("openbsd"))
                    return "openbsd";
                else if (plat.Contains("netbsd"))
                    return "netbsd";
                else if (plat.Contains("dragonfly"))
                    return "dragonflybsd";
                else if (plat.Contains("miros bsd") || plat.Contains("mirbsd"))
                    return "miros bsd";
                else if (plat.Contains("desktopbsd"))
                    return "desktopbsd";
                else if (plat.Contains("darwin"))
                    return "osx";
                else if (plat.Contains("unix"))
                    return "unix";
                else return "toaster (unknown)";
            }
        }

        /// <summary>
        /// Gets the client's browser.
        /// </summary>
        [JsonProperty("$browser")]
        public string Browser
        {
            get
            {
                var a = typeof(DiscordClient).GetTypeInfo().Assembly;
                var an = a.GetName();
                return $"DSharpPlus {an.Version.ToString(4)}";
            }
        }

        /// <summary>
        /// Gets the client's device.
        /// </summary>
        [JsonProperty("$device")]
        public string Device 
            => this.Browser;

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
