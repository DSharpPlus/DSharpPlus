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
    public readonly struct EveryoneMention : IMention {

        //This is pointless because new EveryoneMention() will work, but it is here for consistency with the other mentionables.
        /// <summary>
        /// Allow the mentioning of @everyone and @here. Alias to <see cref="EveryoneMention()"/> constructor.
        /// </summary>
        public static readonly EveryoneMention All = new EveryoneMention();
    }

    /// <summary>
    /// Allows @user pings to mention in the message.
    /// </summary>
    public readonly struct UserMention : IMention
    {
        /// <summary>
        /// Allow mentioning of all users. Alias to <see cref="UserMention()"/> constructor.
        /// </summary>
        public static readonly UserMention All = new UserMention();

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
        /// Allow the mentioning of all roles.  Alias to <see cref="RoleMention()"/> constructor.
        /// </summary>
        public static readonly RoleMention All = new RoleMention();

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
