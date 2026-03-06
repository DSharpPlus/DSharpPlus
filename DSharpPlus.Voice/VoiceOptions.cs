using System;

namespace DSharpPlus.Voice;

/// <summary>
/// Contains settings to control the behaviour of the voice extension.
/// </summary>
public sealed class VoiceOptions
{
    /// <summary>
    /// Indicates whether to log debugging messages from native MLS code. Defaults to false.
    /// </summary>
    /// <remarks>
    /// Please note that native code doesn't log what session a message was sent from, so any logs sent through this
    /// mechanism are fairly useless if the bot is in two voice connections at once.
    /// </remarks>
    public bool LogNativeMlsDebugMessages { get; set; } = false;

    /// <summary>
    /// Specifies a function to get the reconnection delay on a given consecutive attempt to reconnect to the voice gateway.
    /// </summary>
    /// <remarks>
    /// Defaults to backing off by 100ms each attempt, for a maximum of a second of delay.
    /// </remarks>
    public Func<uint, TimeSpan> GetReconnectionDelay { get; set; }
        = (num) => TimeSpan.FromMilliseconds(uint.Min(num * 100, 1000));

    /// <summary>
    /// Specifies the maximum amount of reconnects to attempt consecutively before a voice connection is abandoned. The counter
    /// resets if a connection is successfully established. Defaults to 10.
    /// </summary>
    public uint MaxReconnects { get; set; } = 10;

    /// <summary>
    /// Specifies whether DSharpPlus.Voice should attempt to reconnect automatically, if possible. It will always attempt to
    /// resume a setting, regardless of this setting. Defaults to true.
    /// </summary>
    /// <remarks>
    /// Please note that it may not be possible to manually reconnect if this setting is disabled.
    /// </remarks>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Controls whether we will attempt to use AEAD AES-256 GCM encryption if the voice server and the local hardware support it.
    /// Defaults to true. You should typically avoid setting this option at all, it is only needed in esoteric cases described in 
    /// the remarks below.
    /// </summary>
    /// <remarks>
    /// The library will apply the correct default in virtually all cases, including disabling it if it is reported as unsupported
    /// by the hardware. Only a select few CPUs may erroneously enable it, and that is what this option exists for: <br/>
    /// - VIA x86 CPUs on Linux report themselves as capable, but it is implemented in software. This should be disabled.
    /// Note that these CPUs are not supported by DSharpPlus.Voice - with the exception of the VIA Nano - and may require extra work
    /// to function correctly: please consult the BUILDING.md file and the documentation. <br/>
    /// - Derivatively thereof, Zhaoxin ZX-A through ZX-C CPUs on Linux report themselves as capable, but it is implemented in
    /// software. This should be disabled. ZX-C+ and newer CPUs correctly implement the required instructions. <br/>
    /// - AMD Geode NX CPUs on Linux report themselves as capable, but it is implemented in software. This should be disabled. 
    /// Note that these CPUs are not supported by DSharpPlus.Voice and may require extra work to function correctly: please consult 
    /// the BUILDING.md file and the documentation. <br/> <br/>
    /// While RISC-V CPUs are also prone to misrepresent their AEAD AES-256 GCM hardware support, they will currently not be picked
    /// up by DSharpPlus.Voice at all, and setting this option is currently superfluous. However, it may be wise to disable it for
    /// future-proofing on RISC-V CPUs preceding the year 2022. <br/> <br/>
    /// If you do not control the host hardware your code will run on, it is inadvisable to set this option - cases where the
    /// automatic detection doesn't work properly are esoteric, and chances are not a single one of your users will ever hit this.
    /// </remarks>
    public bool EnableAeadAes256GcmEncryption { get; set; } = true;
}
