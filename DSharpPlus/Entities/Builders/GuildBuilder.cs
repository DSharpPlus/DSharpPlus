using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the builder that will be used to Create a Guild.
    /// </summary>
    public sealed class GuildCreateBuilder : Abstractions.GuildBuilder<GuildCreateBuilder>
    {
        /// <summary>
        /// <para>Gets The list of Roles to be added.</para>
        /// </summary>
        public IReadOnlyCollection<GuildBuilderRole> Roles => this._Roles;

        internal List<GuildBuilderRole> _Roles = new List<GuildBuilderRole>();

        /// <summary>
        /// Gets the list of Channels to be added.
        /// </summary>
        public IReadOnlyCollection<GuildBuilderChannel> Channels => this.Channels;

        internal List<GuildBuilderChannel> _Channels = new List<GuildBuilderChannel>();

        /// <summary>
        /// <para>Sets the roles to be used in the Guild. If you are to add overwrites to the <see cref="Channels"/>, you must supply the Id in <seealso cref="GuildBuilderRole"/> and use it there.</para>
        /// <para>NOTE: If an Id is supplied, it will not be kept, just used as a reference during the creation.</para>
        /// <para>Can only be used during Guild Create.  If a Role is added during Guild Modify, then an error will be thrown during sending to discord.</para>
        /// </summary>
        /// <param name="role">The Role to be added.</param>
        /// <returns></returns>
        public GuildCreateBuilder WithRole(GuildBuilderRole role)
        {
            this._Roles.Add(role);

            return this;
        }

        /// <summary>
        /// <para>Sets the roles to be used in the Guild. If you are to add overwrites to the <see cref="Channels"/>, you must supply the Id in <seealso cref="GuildBuilderRole"/> and use it there.</para>
        /// <para>NOTE: If an Id is supplied, it will not be kept, just used as a reference during the creation.</para>
        /// <para>Can only be used during Guild Create.  If a Role is added during Guild Modify, then an error will be thrown during sending to discord.</para>
        /// </summary>
        /// <param name="roles">The roles to be added.</param>
        /// <returns></returns>
        public GuildCreateBuilder WithRoles(IEnumerable<GuildBuilderRole> roles)
        {
            this._Roles.AddRange(roles);

            return this;
        }

        /// <summary>
        /// <para>Sets the channel to be used in the Guild. If your channel is going to be in a category, you must supply the Id's.</para>
        /// <para>NOTE: If an Id is supplied, it will not be kept, just used as a reference during the creation.</para>
        /// <para>Can only be used during Guild Create.  If a Channel is added during Guild Modify, then an error will be thrown during sending to discord.</para>
        /// </summary>
        /// <param name="channel">The channel to be added.</param>
        /// <returns></returns>
        public GuildCreateBuilder WithChannel(GuildBuilderChannel channel)
        {
            this._Channels.Add(channel);

            return this;
        }

        /// <summary>
        /// <para>Sets the channels to be used in the Guild. If your channel is going to be in a category, you must supply the Id's.</para>
        /// <para>NOTE: If an Id is supplied, it will not be kept, just used as a reference during the creation.</para>
        /// <para>Can only be used during Guild Create.  If a Channel is added during Guild Modify, then an error will be thrown during sending to discord.</para>
        /// </summary>
        /// <param name="channels">The channels to be added.</param>
        /// <returns></returns>
        public GuildCreateBuilder WithChannels(IEnumerable<GuildBuilderChannel> channels)
        {
            this._Channels.AddRange(channels);

            return this;
        }

        /// <summary>
        /// Sends the builder to the DiscordClient to Create the Guild.
        /// </summary>
        /// <param name="client">The DiscordClient to use.</param>
        /// <returns></returns>
        public async Task<DiscordGuild> CreateAsync(DiscordClient client)
        {
            return await client.CreateGuildAsync(this).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            this._Roles.Clear();
            this._Channels.Clear();
        }

        /// <inheritdoc />
        internal override void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
                throw new ArgumentException("You must specify a Name of the guild.");
        }

        /// <summary>
        /// Represents the minified version of Role Properties that can be added.
        /// </summary>
        public class GuildBuilderRole
        {
            /// <summary>
            /// Gets or Sets the User Generated Id if they need to reference the role to a channel or elsewhere
            /// </summary>
            public ulong Id { get; set; }
            /// <summary>
            /// Gets or Sets the name of the role.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Gets or Sets the color for the role.
            /// </summary>
            public Optional<int> Color { get; set; }
            /// <summary>
            /// Gets or Sets if the role should be hoisted.
            /// </summary>
            public bool IsHoisted { get; set; }
            /// <summary>
            /// Gets or Sets the position of the role in the hierarchy.
            /// </summary>
            public Optional<int> Position { get; set; }
            /// <summary>
            /// Gets or Sets the permissions of the role.
            /// </summary>
            public Permissions Permissions { get; set; }
            /// <summary>
            /// Gets or Sets if the role is mentionable.
            /// </summary>
            public bool Mentionable { get; set; } = false;
        }

        /// <summary>
        /// Represents a minified version of a channel that can be created when creating a guild.
        /// </summary>
        public class GuildBuilderChannel
        {
            /// <summary>
            /// Gets or Sets the User Generated Id if they need to reference the channel to a channel or elsewhere
            /// </summary>
            public ulong Id { get; set; }

            /// <summary>
            /// Gets or Sets the name of the Channel
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or Sets the parent Id of the channel
            /// </summary>
            public ulong? ParentId { get; set; }

            /// <summary>
            /// Gets or Sets the Type of the channel
            /// </summary>
            public ChannelType Type { get; set; }

            /// <summary>
            /// Gets or Sets the topic of the channel.
            /// </summary>
            public Optional<string> Topic { get; set; }

            /// <summary>
            /// Gets or Sets the bitrate of the channel
            /// </summary>
            public int? Bitrate { get; set; }

            /// <summary>
            /// Gets or Sets if the channel is nsfw
            /// </summary>
            public bool? Nsfw { get; set; }

            /// <summary>
            /// Gets or Sets the Permission overwrites.
            /// </summary>
            public IEnumerable<ChannelOverwrite> PermissionOverwrites { get; set; }

            /// <summary>
            /// Representts the minified Channel Overwrite that is allowed when creating a guild.
            /// </summary>
            public class ChannelOverwrite
            {
                /// <summary>
                /// Gets or Sets The temp Id of the role.
                /// </summary>
                public ulong Id { get; set; }

                /// <summary>
                /// Gets or Sets the Permissions that are allowed.
                /// </summary>
                public Permissions? AllowPermissions { get; set; }

                /// <summary>
                /// Gets or Sets the Permissions that are Denied.
                /// </summary>
                public Permissions? DenyPermissions { get; set; }
            }
        }
    }

    /// <summary>
    /// Represents the builder that will be used to Modify a Guild.
    /// </summary>
    public sealed class GuildModifyBuilder : Abstractions.GuildBuilder<GuildModifyBuilder>
    {
        /// <summary>
        /// Gets the new Owner of the Guild.
        /// </summary>
        public Optional<DiscordMember> NewOwner { get; internal set; }

        /// <summary>
        /// Gets The new guild splash.
        /// </summary>
        public Optional<Stream> Splash { get; internal set; }

        /// <summary>
        /// Gets or Sets the new guild MFA level.
        /// </summary>
        public Optional<MfaLevel> MfaLevel { get; internal set; }

        /// <summary>
        /// Gets the new guild rules channel.
        /// </summary>
        public Optional<DiscordChannel> RulesChannel { get; internal set; }

        /// <summary>
        /// Gets the new guild public updates channel.
        /// </summary>
        public Optional<DiscordChannel> PublicUpdatesChannel { get; internal set; }

        /// <summary>
        /// <para>Sets the Mfa Level of the guild.</para>
        /// <para>This can only be set during modifing a guild.</para>
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public GuildModifyBuilder WithMfaLevel(MfaLevel level)
        {
            this.MfaLevel = level;

            return this;
        }

        /// <summary>
        /// <para>Sets the new Discord Owner of the guild.</para>
        /// <para>This can only be set during modifing a guild.</para>
        /// </summary>
        /// <param name="owner">the new owner of the guild</param>
        /// <returns></returns>
        public GuildModifyBuilder WithNewOwener(DiscordMember owner)
        {
            this.NewOwner = owner;

            return this;
        }

        /// <summary>
        /// <para>Sets the new Splash of the Guild.</para>  
        /// <para>This can only be set during modify.</para>
        /// </summary>
        /// <param name="stream">The stream of the new splash screen.</param>
        /// <returns></returns>
        public GuildModifyBuilder WithSplash(Stream stream)
        {
            this.Splash = stream;

            return this;
        }

        /// <summary>
        /// <para>Sets the new Rules channel.</para>
        /// <para>This can only be done when modifing a guild.</para>
        /// </summary>
        /// <param name="channel">The channel to set as the rules channel.</param>
        /// <returns></returns>
        public GuildModifyBuilder WithRulesChannel(DiscordChannel channel)
        {
            this.RulesChannel = channel;

            return this;
        }

        /// <summary>
        /// <para>Sets the new public updates channel.</para>
        /// <para>This can only be done when modifing a guild.</para>
        /// </summary>
        /// <param name="channel">The channel to set as the public updates channel.</param>
        /// <returns></returns>
        public GuildModifyBuilder WithPublicUpdatesChannel(DiscordChannel channel)
        {
            this.PublicUpdatesChannel = channel;

            return this;
        }

        /// <summary>
        /// Sends the builder to the guild to modify it.
        /// </summary>
        /// <param name="guild">The guild to modify.</param>
        /// <returns></returns>
        public async Task<DiscordGuild> ModifyAsync(DiscordGuild guild)
        {
            return await guild.ModifyAsync(this).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            this.MfaLevel = Optional.FromNoValue<MfaLevel>();
            this.NewOwner = Optional.FromNoValue<DiscordMember>();
            this.Splash = Optional.FromNoValue<Stream>();
            this.RulesChannel = Optional.FromNoValue<DiscordChannel>();
            this.PublicUpdatesChannel = Optional.FromNoValue<DiscordChannel>();
        }

        /// <inheritdoc />
        internal override void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
                throw new ArgumentException("You must specify a Name of the guild.");
        }
    }
}
