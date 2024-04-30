using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "osx";
            }

            string plat = RuntimeInformation.OSDescription.ToLowerInvariant();
            return plat.Contains("freebsd")
                ? "freebsd"
                : plat.Contains("openbsd")
                    ? "openbsd"
                    : plat.Contains("netbsd")
                                    ? "netbsd"
                                    : plat.Contains("dragonfly")
                                                    ? "dragonflybsd"
                                                    : plat.Contains("miros bsd") || plat.Contains("mirbsd")
                                                                    ? "miros bsd"
                                                                    : plat.Contains("desktopbsd")
                                                                                    ? "desktopbsd"
                                                                                    : plat.Contains("darwin") ? "osx" : plat.Contains("unix") ? "unix" : "toaster (unknown)";
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
            Assembly a = typeof(DiscordClient).GetTypeInfo().Assembly;
            AssemblyName an = a.GetName();
            return $"DSharpPlus {an.Version.ToString(4)}";
        }
    }

    /// <summary>
    /// Gets the client's device.
    /// </summary>
    [JsonProperty("$device")]
    public string Device
        => Browser;

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
