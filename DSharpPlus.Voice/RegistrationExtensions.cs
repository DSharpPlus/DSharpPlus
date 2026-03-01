using System;

using DSharpPlus.Extensions;
using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.Transport;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides the registration methods for DSharpPlus.Voice.
/// </summary>
public static class RegistrationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers DSharpPlus.Voice to the given service collection.
        /// </summary>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddVoiceExtension()
        {
            services.AddScoped<IMediaTransportService, MediaTransportService>()
                .AddScoped<ITransportService, TransportService>()
                .AddScoped<ICryptorFactory, DefaultCryptorFactory>()
                .AddScoped<IAudioCodec, OpusCodec>()
                .AddScoped<IE2EESession, MlsSession>();

            services.ConfigureEventHandlers(x => x.AddEventHandlers<VoiceInitializer>());

            return services;
        }
    }

    extension(DiscordClientBuilder builder)
    {
        /// <summary>
        /// Registers DSharpPlus.Voice to the given client builder.
        /// </summary>
        /// <returns>The builder instance for chaining.</returns>
        public DiscordClientBuilder UseVoice()
        {
            builder.ConfigureServices(s => s.AddVoiceExtension());
            return builder;
        }

        /// <summary>
        /// Configures the voice extension.
        /// </summary>
        /// <returns>The builder instance for chaining.</returns>
        public DiscordClientBuilder ConfigureVoice(Action<VoiceOptions> configure)
        {
            builder.ConfigureServices(s => s.Configure(configure));
            return builder;
        }
    }
}