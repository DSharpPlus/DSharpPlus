using System.Threading.Tasks;

using DSharpPlus.EventArgs;
using DSharpPlus.Voice.Interop;
using DSharpPlus.Voice.Interop.Koana;
using DSharpPlus.Voice.Interop.Sodium;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Voice;

internal sealed class VoiceInitializer : IEventHandler<ClientStartedEventArgs>
{
    private readonly ILogger<KoanaContext> koanaLogger;
    private readonly ILogger<VoiceInitializer> logger;
    private readonly bool logNativeMessages;

    public VoiceInitializer
    (
        IOptions<VoiceOptions> options,
        ILogger<KoanaContext> koanaLogger,
        ILogger<VoiceInitializer> logger
    )
    {
        this.koanaLogger = koanaLogger;
        this.logger = logger;
        this.logNativeMessages = options.Value.LogNativeMlsDebugMessages;
    }

    public Task HandleEventAsync(DiscordClient _, ClientStartedEventArgs __)
    {
        KoanaInterop.SetLogger(this.koanaLogger, this.logNativeMessages);
        KoanaInterop.Initialize();

        if (!OpenSslVersionCheck.CheckOpenSslVersionCompatible())
        {
            this.logger.LogError("The OpenSSL version installed on your system is incompatible with DSharpPlus.Voice. " +
                "Please provide OpenSSL 3.x or greater either system-wide or by placing an OpenSSL 3.x libcrypto in the execution directory of your bot.");
        }

        SodiumInterop.Initialize();

        if (!SodiumInterop.IsAeadAes256GcmAvailable())
        {
            this.logger.LogWarning("The current hardware or sodium build is not compatible with AEAD AES-256 GCM, the preferred transport encryption. " +
                "DSharpPlus.Voice will fall back to AEAD XChaCha20 Poly1305 transport encryption.");
        }

        return Task.CompletedTask;
    }
}
