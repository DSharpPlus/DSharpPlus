using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the builder that will be used to Create a Channel.
    /// </summary>
    public sealed class ChannelCreateBuilder : Abstractions.ChannelBuilder<ChannelCreateBuilder>
    {
        /// <summary>
        /// Gets the type of channel that you want to set.
        /// </summary>
        public Optional<ChannelType> Type { get; internal set; }

        /// <summary>
        /// <para>Sets the Type of the channel.</para>
        /// <para>This can only be set during creation.</para>
        /// </summary>
        /// <param name="type">The type of the Channel</param>
        /// <returns></returns>
        public ChannelCreateBuilder WithType(ChannelType type)
        {
            this.Type = type;

            return this;
        }

        /// <summary>
        /// Creates a Channel utilizing what was specified to the builder.
        /// </summary>
        /// <param name="guild">The guild to create the channel in.</param>
        /// <returns></returns>
        public async Task<DiscordChannel> CreateAsync(DiscordGuild guild)
        {
            return await guild.CreateChannelAsync(this).ConfigureAwait(false);
        }

        /// <inheritdoc />
        internal override void Validate()
        {
            if(this.Type == ChannelType.Group || this.Type == ChannelType.Unknown)
                throw new ArgumentException("Channel type must be text, voice, or category.", nameof(this.Type));

            if (this.Type == ChannelType.Category && this.ParentId != null)
                throw new ArgumentException("Cannot specify parent of a channel category.", nameof(ParentId));
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            this.Type = Optional.FromNoValue<ChannelType>();
        }
    }

    /// <summary>
    /// Represents the builder that will be used to Modify a Channel.
    /// </summary>
    public sealed class ChannelModifyBuilder : Abstractions.ChannelBuilder<ChannelModifyBuilder>
    {
        /// <summary>
        /// Sends the changes of the channel to Discord.
        /// </summary>
        /// <param name="channel">The channel the builder should be executed against.</param>
        /// <returns></returns>
        public async Task ModifyAsync(DiscordChannel channel)
        {
            await channel.ModifyAsync(this).ConfigureAwait(false);
        }

        /// <inheritdoc />
        internal override void Validate()
        {
            
        }
    }
}
