using System.Runtime.InteropServices;
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
    private readonly bool userEnabledAeadAes256Gcm;

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
        this.userEnabledAeadAes256Gcm = options.Value.EnableAeadAes256GcmEncryption;
    }

    public Task HandleEventAsync(DiscordClient _, ClientStartedEventArgs __)
    {
        KoanaInterop.SetLogger(this.koanaLogger, this.logNativeMessages);
        KoanaInterop.Initialize();

        if (!OpenSslVersionCheck.CheckOpenSslVersionCompatible())
        {
            this.logger.LogError("The OpenSSL version installed on your system is incompatible with DSharpPlus.Voice. "
                + "Please provide OpenSSL 3.x or greater either system-wide or by placing an OpenSSL 3.x libcrypto in the execution directory of your bot.");
        }

        SodiumInterop.Initialize();

        if (!SodiumInterop.IsAeadAes256GcmAvailable())
        {
            this.logger.LogWarning("The current hardware or sodium build is not compatible with AEAD AES-256 GCM, the preferred transport encryption. "
                + "DSharpPlus.Voice will fall back to AEAD XChaCha20 Poly1305 transport encryption.");
        }
        else if (!this.userEnabledAeadAes256Gcm)
        {
            this.logger.LogWarning("AEAD AES-256 GCM encryption was disabled by the bot. Please verify doing so is necessary for your hardware. "
                + "DSharpPlus.Voice will fall back to AEAD XChaCha20 Poly1305 transport encryption.");
        }
        else
        {
            this.logger.LogInformation("AEAD AES-256 GCM encryption support enabled.");
        }

        this.logger.LogInformation("Initialized DSharpPlus.Voice for process architecture {arch}.", RuntimeInformation.ProcessArchitecture);

        return Task.CompletedTask;
    }
}
