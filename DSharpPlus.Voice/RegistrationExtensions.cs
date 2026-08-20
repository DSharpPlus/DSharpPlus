using System;

using DSharpPlus.Extensions;
using DSharpPlus.Voice.AudioWriters;
using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.Metrics;
using DSharpPlus.Voice.Receivers;
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
                .AddScoped<IAudioWriterFactory, DefaultAudioWriterFactory>()
                .AddScoped<IE2EESession, MlsSession>()
                .AddScoped<VoiceMetrics>();

            services.AddSingleton<IVoiceConnectionRepository, VoiceConnectionRepository>();
            
            // receiver types
            services.AddScoped<DefaultAudioReceiver>()
                .AddScoped<NullAudioReceiver>();

            services.ConfigureEventHandlers(x => x.AddEventHandlers<VoiceInitializer>()
                .AddEventHandlers<GuildMonitoringEventHandler>(ServiceLifetime.Singleton));

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

        /// <summary>
        /// Enables seamlessly connecting to a voice channel even if a connection to another voice channel in the same guild already exists.
        /// </summary>
        /// <remarks>
        /// While it may simplify your code to not have to explicitly disconnect from the other voice channel, do note that this will silently 
        /// invalidate the preexisting connection, with all consequences that entails.
        /// </remarks>
        /// <returns>The builder instance for chaining.</returns>
        public DiscordClientBuilder EnableSeamlessVoiceReconnecting()
        {
            builder.ConfigureServices(s => s.Replace<IVoiceConnectionRepository, ImplicitlyReconnectingVoiceConnectionRepository>());
            return builder;
        }
    }
}
