using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the Channel that will be Created or Modified.
    /// </summary>
    public abstract class ChannelBuilder<T>
    {
        /// <summary>
        /// <para>Gets or Sets the Name of the channel to be sent.</para>
        /// <para>This must be between 2 and 100 Characters and is Required for Guild Creation.</para>
        /// </summary>
        public string Name
        {
            get => this._name;
            set
            {
                if (value != null && value.Length <= 2 && value.Length >= 100)
                    throw new ArgumentException("Name must be between 2 and 100 Characters.", nameof(value));
                this._name = value;
            }
        }
        private string _name;

        /// <summary>
        /// Gets the topic of the channel.
        /// </summary>
        public Optional<string> Topic { get; internal set; }

        /// <summary>
        /// Gets the Bitrate of the voice channel.
        /// </summary>
        public Optional<int> Bitrate { get; internal set; }

        /// <summary>
        /// Gets the User Limit of the voice channel.
        /// </summary>
        public Optional<int> UserLimit { get; internal set; }

        /// <summary>
        /// Gets the rate limit of the text chanel.
        /// </summary>
        public Optional<int> RateLimit { get; internal set; }

        /// <summary>
        /// Gets the Position of the channel.
        /// </summary>
        public Optional<int> Position { get; internal set; }

        /// <summary>
        /// Gets the Collection of Overwrites the channel should have.
        /// </summary>
        public IReadOnlyCollection<DiscordOverwriteBuilder> Overwrites => this._Overwrites;

        internal List<DiscordOverwriteBuilder> _Overwrites = new List<DiscordOverwriteBuilder>();

        /// <summary>
        /// Gets the ParentId of the Channel
        /// </summary>
        public Optional<ulong> ParentId { get; set; }

        /// <summary>
        /// Gets if the text/news/store channel is nsfw.
        /// </summary>
        public bool Nsfw { get; set; }

        /// <summary>
        /// Gets the AuditLog Reason for modifing the channel.
        /// </summary>
        public Optional<string> AuditLogReason { get; internal set; }

        /// <summary>
        /// Sets the name of the channel.
        /// </summary>
        /// <param name="name">The name to give.</param>
        /// <returns></returns>
        public T WithName(string name)
        {
            this.Name = name;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// <para>Sets the topic of the channel.</para>
        /// <para>This can only be done on channels that are <see cref="ChannelType.Text"/> or <see cref="ChannelType.News"/></para>
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <returns></returns>
        public T WithTopic(string topic)
        {
            this.Topic = topic;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// <para>Sets the bitrate of the channel.</para>
        /// <para>Can only be set for Voice Channels.</para>
        /// </summary>
        /// <param name="bit">The bitrate</param>
        /// <returns></returns>
        public T WithBitrate(int bit)
        {
            this.Bitrate = bit;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// <para>Sets the Userlimit of the Channel.</para>
        /// <para>This can only be set for <see cref="ChannelType.Voice"/></para>
        /// </summary>
        /// <param name="users">The amount of users</param>
        /// <returns></returns>
        public T WithUserLimit(int users)
        {
            this.UserLimit = users;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// <para>Sets the ratelimt that each user can send a message</para>
        /// <para>This can only be done in <see cref="ChannelType.Text"/></para>
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public T WithRateLimit(int rate)
        {
            this.RateLimit = rate;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the position of the channel.
        /// </summary>
        /// <param name="position">Sets the position of the channel.</param>
        /// <returns></returns>
        public T WithPosition(int position)
        {
            this.Position = position;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets a Builder on the channel.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public T WithOverwrite(DiscordOverwriteBuilder builder)
        {
            this._Overwrites.Add(builder);

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the builders on the channel.
        /// </summary>
        /// <param name="builders"></param>
        /// <returns></returns>
        public T WithOverwrites(IEnumerable<DiscordOverwriteBuilder> builders)
        {
            this._Overwrites.AddRange(builders);

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// <para>Sets the Parent of the channel.</para>
        /// <para>This can be done on all channels except <see cref="ChannelType.Category"/></para>
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public T WithParentId(ulong parent)
        {
            this.ParentId = parent;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// <para>Sets the Parent of the channel.</para>
        /// <para>This can be done on all channels except <see cref="ChannelType.Category"/></para>
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public T WithParentId(DiscordChannel parent)
        {
            this.ParentId = parent.Id;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// <para>Sets if the channel is nsfw</para>
        /// <para>This is only valid on channels of type <see cref="ChannelType.News"/>,<see cref="ChannelType.Store"/>, or <see cref="ChannelType.Text"/></para>
        /// </summary>
        /// <param name="nsfw"></param>
        /// <returns></returns>
        public T WithNsfw(bool nsfw)
        {
            this.Nsfw = nsfw;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the reason for the Change.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public T WithAuditLogReason(string reason)
        {
            this.AuditLogReason = reason;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Clears the contents of this builder.
        /// </summary>
        public virtual void Clear()
        {
            this._name = "";
            this.Topic = "";
            this.Position = Optional.FromNoValue<int>();
            this.RateLimit = Optional.FromNoValue<int>();
            this.Bitrate = Optional.FromNoValue<int>();
            this.ParentId = Optional.FromNoValue<ulong>();
            this._Overwrites.Clear();
            this.UserLimit = Optional.FromNoValue<int>();
            this.Nsfw = false;
        }

        /// <summary>
        /// Performs validation logic to verify all the input is valid before sending it off to discord.
        /// </summary>
        internal abstract void Validate();
    }

    /// <summary>
    /// Represents the builder that will be used to Create a Channel.
    /// </summary>
    public sealed class ChannelCreateBuilder : ChannelBuilder<ChannelCreateBuilder>
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
    public sealed class ChannelModifyBuilder : ChannelBuilder<ChannelModifyBuilder>
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
