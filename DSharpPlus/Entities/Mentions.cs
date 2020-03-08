using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Interface for mentionables
    /// </summary>
    public interface IMention { }

    /// <summary>
    /// Allows @everyone and @here pings to mention in the message.
    /// </summary>
    public readonly struct EveryoneMention : IMention { }

    /// <summary>
    /// Allows @user pings to mention in the message.
    /// </summary>
    public readonly struct UserMention : IMention
    {
        /// <summary>
        /// Optional Id of the user that is allowed to be mentioned. If null, then all user mentions will be allowed. 
        /// </summary>
        public ulong? Id { get; }

        /// <summary>
        /// Allows the specific user to be mentioned
        /// </summary>
        /// <param name="id"></param>
        public UserMention(ulong id) { this.Id = id;  }

        /// <summary>
        /// Allows the specific user to be mentioned
        /// </summary>
        /// <param name="user"></param>
        public UserMention(DiscordUser user) : this(user.Id) { }

        public static implicit operator UserMention(DiscordUser user) => new UserMention(user.Id);
    }

    /// <summary>
    /// Allows @role pings to mention in the message.
    /// </summary>
    public readonly struct RoleMention : IMention
    {
        /// <summary>
        /// Optional Id of the role that is allowed to be mentioned. If null, then all role mentions will be allowed. 
        /// </summary>
        public ulong? Id { get; }

        /// <summary>
        /// Allows the specific id to be mentioned
        /// </summary>
        /// <param name="id"></param>
        public RoleMention(ulong id) { this.Id = id; }

        /// <summary>
        /// Allows the specific role to be mentioned
        /// </summary>
        /// <param name="role"></param>
        public RoleMention(DiscordRole role) : this(role.Id) { }

        public static implicit operator RoleMention(DiscordRole role) => new RoleMention(role.Id);
    }


}
